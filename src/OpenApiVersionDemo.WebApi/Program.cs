using OpenApiVersionDemo.Common;
using OpenApiVersionDemo.WebApi;
using OpenApiVersionDemo.WebApi.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using System.Reflection;
using OpenApiVersionDemo.Services;
using OpenApiVersionDemo.Services.Interfaces;

var versionStrategy = VersionStrategy.Path;

var builder = WebApplication.CreateBuilder(args);

// Use intermediate builder to obtain configuration etc

var intermediateBuilder = WebApplication.CreateBuilder(args);

var intermediateApp = intermediateBuilder.Build();

// Add services to the container.

var webHostEnvironment = intermediateApp.Services.GetRequiredService<IWebHostEnvironment>();

var nLogLogger = new NLogLogger(webHostEnvironment.EnvironmentName);

builder.Services.AddSingleton<ILogger>(nLogLogger);

builder.Services.AddTransient<ITestService, TestService>();

builder.Services.AddTransient<SwaggerAuthenticationMiddleware>();

builder.Services.AddControllers();

builder.Services.AddVersionedApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";

    // Value applies if using URL segment versioning

    o.SubstituteApiVersionInUrl = true;

    // ApiVersionParameterSource prevents the version
    // parameters from showing up in swagger UI, and is also required specifically where
    // VersionStrategy.Header is used

    if (versionStrategy == VersionStrategy.Path)
    {
        o.ApiVersionParameterSource = new UrlSegmentApiVersionReader();
    }
    else if (versionStrategy == VersionStrategy.Header)
    {
        o.ApiVersionParameterSource = new HeaderApiVersionReader();
    }
    else
    {
        throw new InvalidOperationException($"Unhandled value for {nameof(versionStrategy)}");
    }
});

var configuration = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.{webHostEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: false)
                    .Build();

string apiVersionDisplayName = configuration["App:DisplayName"];

IList<ApiVersionDescriptor> apiVersionDescriptors = new List<ApiVersionDescriptor>()
{
    new (apiVersionDisplayName, "v1.0", deprecated: false),
    new (apiVersionDisplayName, "v0.1", deprecated: true)
};

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;

    // Header, media type and query string API versioning supported
    // So use any of the following to specify API version:
    //
    //                url-segment: /v1.0/path
    //                header: Api-Version: 1.0
    //                media-type: Accept: application/json;v=1.0
    //                querystring: ?api-version=0.1

    // Set ApiVersionReader. Multiple options can be combined with ApiVersionReader.Combine

    if (versionStrategy == VersionStrategy.Path)
    {
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    }
    else if (versionStrategy == VersionStrategy.Header)
    {
        o.ApiVersionReader = new HeaderApiVersionReader("Api-Version");
    }
    else
    {
        throw new InvalidOperationException($"Unhandled value for {nameof(versionStrategy)}");
    }
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(x => x.FullName);

    foreach (var apiVersionDescriptor in apiVersionDescriptors)
    {
        options.SwaggerDoc(apiVersionDescriptor.VersionShort, new OpenApiInfo { Title = apiVersionDescriptor.VersionFull, Version = apiVersionDescriptor.VersionShort });
    }

    // Enabling bearer token auth in Swagger UI https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1425 (zscao commented on 31 Mar 2020)

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Use value 'Bearer [accessToken]')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer"
    });

    options.MapType<FileResult>(() => new OpenApiSchema() { Type = "file" });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        }, new List<string>()
                    }
                });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();

        // Unversioned actions should be included everywhere

        if (actionApiVersionModel == null)
        {
            return true;
        }
        if (actionApiVersionModel.DeclaredApiVersions.Any())
        {
            return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
        }

        return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
    });

    if (versionStrategy == VersionStrategy.Path)
    {
        // Use these filters for path versioning (version)

        options.OperationFilter<RemoveVersionParameterFilter>();
        options.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
    }
    else if (versionStrategy == VersionStrategy.Header)
    {
        // Use this filter for header versioning (Api-Version)

        options.OperationFilter<SwaggerAddHeaderParameterOperationFilter>();
    }
    else
    {
        throw new InvalidOperationException($"Unhandled value for {nameof(versionStrategy)}");
    }

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // Requires <GenerateDocumentationFile>true</GenerateDocumentationFile> in .csproj file for XML document generation

    options.IncludeXmlComments(xmlPath);
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

// Configure logging used by ASP.NET Core. Set minimum log levels in JSON app configuration

builder.Logging.ClearProviders();
builder.Logging.AddProvider(new NLogLoggerProvider(webHostEnvironment.EnvironmentName));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<SwaggerAuthenticationMiddleware>();

// Enable middleware to serve generated Swagger as a JSON endpoint.

app.UseSwagger(c =>
{
    c.RouteTemplate = "api/docs/swagger/{documentName}/swagger.json";
});

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.

// NOTE: Missing HTTP verb attributes on controller actions will cause "Fetch error" (HTTP 500) when loading Swagger UI

app.UseSwaggerUI(c =>
{
    foreach (var apiVersionDescriptor in apiVersionDescriptors)
    {
        c.SwaggerEndpoint($"swagger/{apiVersionDescriptor.VersionShort}/swagger.json", apiVersionDescriptor.VersionFull);
    }

    // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=netcore-cli

    c.RoutePrefix = "api/docs";
});

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
