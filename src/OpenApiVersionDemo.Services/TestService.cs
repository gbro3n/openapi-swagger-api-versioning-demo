using OpenApiVersionDemo.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace OpenApiVersionDemo.Services;

public class TestService : ITestService
{
    private readonly ILogger _logger;

    public TestService(ILogger logger)
    {
        _logger = logger;
    }

    public string GetTestString(int length)
    {
        var random = new Random();

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string testString = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

        _logger.LogInformation($"{nameof(testString)}: {testString}");

        return testString;
    }
}