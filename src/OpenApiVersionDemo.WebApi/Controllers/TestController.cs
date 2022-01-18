using OpenApiVersionDemo.Services.Interfaces;
using OpenApiVersionDemo.WebApi.ApiModels;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiVersionDemo.WebApi.Controllers;

[ApiController]
[Route("api/test")]
[ApiVersion("1.0")]
[ApiVersion("0.1")]
public class TestController : ControllerBase
{
    private readonly ITestService _testService;

    public TestController(ITestService testService)
    {
        _testService = testService;
    }

    [MapToApiVersion("0.1")]
    [HttpGet("version-test")]
    public ActionResult TestApiVersionHeader0_1(int length)
    {
        var testString = _testService.GetTestString(length);

        var testStringContainer = new TestStringContainer
        {
            TestString = testString
        };

        return Ok(testStringContainer);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("version-test")]
    public ActionResult TestApiVersionHeader1_0(int length)
    {
        var testString = _testService.GetTestString(length);

        var testStringContainer = new TestStringContainer
        {
            TestString = testString
        };

        return Ok(testStringContainer);
    }

    [MapToApiVersion("0.1")]
    [HttpGet("v{version:apiVersion}/version-test")]
    public ActionResult TestApiVersionPath0_1(int length)
    {
        var testString = _testService.GetTestString(length);

        var testStringContainer = new TestStringContainer
        {
            TestString = testString
        };

        return Ok(testStringContainer);
    }

    [MapToApiVersion("1.0")]
    [HttpGet("v{version:apiVersion}/version-test")]
    public ActionResult TestApiVersionPath1_0(int length)
    {
        var testString = _testService.GetTestString(length);

        var testStringContainer = new TestStringContainer
        {
            TestString = testString
        };

        return Ok(testStringContainer);
    }
}