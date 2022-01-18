# OpenAPI Swagger API Versioning Demo in .NET Core 6

Demonstrates an OpenAPI (Swagger) implementation in .NET Core 6 including API versioning and Basic authentication.

Full post: https://garethbrown.net/2022/01/18/api-versioning-and-security-with-swagger-swashbuckle-in-net-core-6.html

- An OpenAPI / Swagger configuration in `Program.cs`
- API versioning with an option for switching between URL and HTTP header versioning strategies
- Implementations of IOperationFilter and IDocumentFilter to manipulate the parameter options and values passed in requests to API from the Swagger UI
- Bearer token configuration for OAuth protection of API endpoints (note that OAuth is not fully implemented for this example, only the Swagger configuration)
- Basic authentication for the Swagger UI