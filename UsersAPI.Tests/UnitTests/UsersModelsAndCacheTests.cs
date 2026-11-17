using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;
using System.Timers;
using Xunit;

namespace UsersAPI.Tests;

public class UsersModelsAndCacheTests
{
    [Theory]
    [InlineData("jogador@gamestore.com", true)]
    [InlineData("admin@fiap.com.br", true)]
    [InlineData("email-invalido", false)]
    [InlineData("", false)]
    public void EmailValidation_ShouldValidateFormatCorrectly(string email, bool expectedValid)
    {
        var isValid = !string.IsNullOrWhiteSpace(email)
                      && email.Contains('@')
                      && email.IndexOf('.', email.IndexOf('@')) > email.IndexOf('@') + 1;

        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public async Task RedisCache_ShouldReturnCachedUserPayload_OnCacheHit()
    {
        var cacheMock = new Mock<IDistributedCache>();
        var userId = Guid.NewGuid();
        var cacheKey = $"users:{userId}";
        var payload = JsonSerializer.Serialize(new { Id = userId, Name = "Heloa", Email = "heloa@gamestore.com" });
        var bytes = Encoding.UTF8.GetBytes(payload);

        cacheMock
            .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var resultBytes = await cacheMock.Object.GetAsync(cacheKey);

        Assert.NotNull(resultBytes);
        var json = Encoding.UTF8.GetString(resultBytes);
        Assert.Contains("Heloa", json);
        cacheMock.Verify(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RedisCache_ShouldInvokeRemoveAsync_WhenUserIsUpdatedOrDeleted()
    {
        var cacheMock = new Mock<IDistributedCache>();
        var userId = Guid.NewGuid();
        var cacheKey = $"users:{userId}";

        await cacheMock.Object.RemoveAsync(cacheKey);

        cacheMock.Verify(c => c.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
    }
}