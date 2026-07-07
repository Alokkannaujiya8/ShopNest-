using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using ShopNest.API.Middleware;
using Xunit;

namespace ShopNest.Tests;

public sealed class SecurityAndLoggingTests
{
    [Fact]
    public void BCrypt_HashPassword_ShouldVerifyCorrectly()
    {
        // Arrange
        var password = "SuperSecurePassword123!";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var isValid = BCrypt.Net.BCrypt.Verify(password, hash);
        var isInvalid = BCrypt.Net.BCrypt.Verify("WrongPassword!", hash);

        // Assert
        Assert.NotEmpty(hash);
        Assert.True(isValid);
        Assert.False(isInvalid);
    }

    [Theory]
    [InlineData("{\"password\":\"mypassword\"}", "{\"password\": \"[REDACTED]\"}")]
    [InlineData("{\"username\":\"alok\",\"password\":\"mypassword\"}", "{\"username\":\"alok\",\"password\": \"[REDACTED]\"}")]
    [InlineData("{\"otp\":\"123456\",\"token\":\"xyz\"}", "{\"otp\": \"[REDACTED]\",\"token\": \"[REDACTED]\"}")]
    public void ExceptionHandlingMiddleware_RedactSensitiveBody_ShouldMaskSecrets(string input, string expected)
    {
        // Arrange
        var method = typeof(ExceptionHandlingMiddleware)
            .GetMethod("RedactSensitiveBody", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        // Act
        var result = method.Invoke(null, new object[] { input }) as string;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36", "Chrome", "Desktop", "Windows")]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1", "Safari", "Mobile", "iOS")]
    [InlineData("Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36", "Chrome", "Mobile", "Android")]
    [InlineData("", "Unknown", "Unknown", "Unknown")]
    public void DbContext_ParseUserAgent_ShouldDetectOSBrowserDevice(string userAgent, string expectedBrowser, string expectedDevice, string expectedOS)
    {
        // Arrange
        // Instantiate a fake/mock of ShopNestDbContext where we can call the private method
        var dbContextType = typeof(ShopNest.Infrastructure.Persistence.ShopNestDbContext);
        var method = dbContextType.GetMethod("ParseUserAgent", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        // Create dbContext instance using reflection or formattable constructor mock
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ShopNest.Infrastructure.Persistence.ShopNestDbContext>().Options;
        var mockContext = new ShopNest.Infrastructure.Persistence.ShopNestDbContext(options, null);

        // Act
        var result = method.Invoke(mockContext, new object[] { userAgent });
        Assert.NotNull(result);

        // Assert (ParseTuple structure returned by ParseUserAgent)
        var browser = result.GetType().GetField("Item1")?.GetValue(result);
        var device = result.GetType().GetField("Item2")?.GetValue(result);
        var os = result.GetType().GetField("Item3")?.GetValue(result);

        Assert.Equal(expectedBrowser, browser);
        Assert.Equal(expectedDevice, device);
        Assert.Equal(expectedOS, os);
    }

    [Fact]
    public async Task SecurityHeadersMiddleware_ShouldAddSecurityHeadersToResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextMock = new Mock<RequestDelegate>();
        nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        var middleware = new SecurityHeadersMiddleware(nextMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("Content-Security-Policy"));
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("strict-origin-when-cross-origin", context.Response.Headers["Referrer-Policy"]);
    }
}
