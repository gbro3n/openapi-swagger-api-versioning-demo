using System.Net;
using System.Text;

namespace OpenApiVersionDemo.WebApi.Middleware;

public class SwaggerAuthenticationMiddleware : IMiddleware
{
    private readonly string _userName;
    private readonly string _password;

    public SwaggerAuthenticationMiddleware(IConfiguration configuration)
    {
        _userName = configuration.GetValue<string>("App:Swagger:BasicUiAuth:Username");
        _password = configuration.GetValue<string>("App:Swagger:BasicUiAuth:Password");
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        //If we hit the swagger locally (in development) then skip basic auth

        if (context.Request.Path.StartsWithSegments("/api/docs") && !IsLocalRequest(context))
        {
            string authHeader = context.Request.Headers["Authorization"];

            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                int splitAuthHeaderParts = 2;

                string[] splitAuthHeader = authHeader.Split(' ', splitAuthHeaderParts, StringSplitOptions.RemoveEmptyEntries);

                if (splitAuthHeader.Length != splitAuthHeaderParts)
                {
                    throw new InvalidOperationException($"{splitAuthHeader} length should be {splitAuthHeaderParts}");
                }

                // Get the encoded username and password
                var encodedUsernamePassword = splitAuthHeader[1].Trim();

                // Decode from Base64 to string

                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                string[] splitDecodedUsernamePassword = decodedUsernamePassword.Split(':', 2);

                // Split username and password
                var username = splitDecodedUsernamePassword[0];
                var password = splitDecodedUsernamePassword[1];

                // Check if login is correct
                if (IsAuthorized(username, password))
                {
                    await next.Invoke(context);
                    return;
                }
            }

            // Return authentication type (causes browser to show login dialog)
            context.Response.Headers["WWW-Authenticate"] = "Basic";

            // Return unauthorized
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        else
        {
            await next.Invoke(context);
        }
    }

    private bool IsAuthorized(string username, string password) => _userName == username && _password == password;

    private bool IsLocalRequest(HttpContext context)
    {
        if (context.Request.Host.Value.StartsWith("localhost:"))
        {
            return true;
        }

        // Handle running using the Microsoft.AspNetCore.TestHost and the site being run entirely locally in memory without an actual TCP/IP connection
        if (context.Connection.RemoteIpAddress == null && context.Connection.LocalIpAddress == null)
        {
            return true;
        }

        if (context.Connection.RemoteIpAddress != null && context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress))
        {
            return true;
        }

        return context.Connection.RemoteIpAddress != null && IPAddress.IsLoopback(context.Connection.RemoteIpAddress);
    }
}
