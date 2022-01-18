namespace OpenApiVersionDemo.WebApi;

public class ApiVersionDescriptor
{
    public ApiVersionDescriptor(string displayName, string versionShort, bool deprecated)
    {
        this.VersionShort = versionShort;
        this.VersionFull = $"{displayName} {versionShort}";
        this.Description = deprecated ? "Deprecated" : null;
    }

    public string VersionShort { get; }

    public string VersionFull { get; }

    public string? Description { get; }
}
