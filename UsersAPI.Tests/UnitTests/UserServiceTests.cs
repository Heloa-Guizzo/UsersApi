using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using UsersAPI.DTOs;
using UsersAPI.Exceptions;
using UsersAPI.Models;
using UsersAPI.Repositories;
using UsersAPI.Services;
using Xunit;

namespace UsersAPI.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _service = new UserService(
            _repositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserHashPasswordAndInvalidateCache_WhenRequestIsValid()
    {
        var request = new RegisterUserRequest("Heloa Guizzo", "heloa@gamestore.com", "Password@123");

        _repositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(false);

        var result = await _service.RegisterAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Heloa Guizzo", result.Name);
        Assert.Equal("heloa@gamestore.com", result.Email);
        Assert.Equal("User", result.Role);
        Assert.NotEqual("Password@123", result.Password);

        _repositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "heloa@gamestore.com")), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("users:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        var request = new RegisterUserRequest("Heloa", "duplicado@gamestore.com", "Password@123");

        _repositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email)).ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(request));

        Assert.Equal("Email already registered.", exception.Message);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllWithCacheInfoAsync_ShouldReturnUsersFromCacheWithCacheHitTrue_WhenCacheExists()
    {
        var cachedUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "User Cached", Email = "cached@gamestore.com", Role = "User" }
        };
        var serializedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cachedUsers));

        _cacheMock
            .Setup(c => c.GetAsync("users:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializedBytes);

        var (users, isCacheHit) = await _service.GetAllWithCacheInfoAsync();

        Assert.True(isCacheHit);
        Assert.Single(users);
        Assert.Equal("User Cached", users.First().Name);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Never); // Não bate no banco quando dá Cache Hit
    }

    [Fact]
    public async Task GetAllWithCacheInfoAsync_ShouldFetchFromRepositoryAndPopulateCache_WhenCacheMissOccurs()
    {
        var dbUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "User DB", Email = "db@gamestore.com", Role = "User" }
        };

        _cacheMock
            .Setup(c => c.GetAsync("users:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(dbUsers);

        var (users, isCacheHit) = await _service.GetAllWithCacheInfoAsync();

        Assert.False(isCacheHit);
        Assert.Single(users);
        Assert.Equal("User DB", users.First().Name);
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            "users:all",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUserFromRepository()
    {
        var expectedUser = new User { Id = Guid.NewGuid(), Name = "Player 1", Email = "player1@gamestore.com" };
        _repositoryMock.Setup(r => r.GetByEmailAsync("player1@gamestore.com")).ReturnsAsync(expectedUser);

        var result = await _service.GetByEmailAsync("player1@gamestore.com");

        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserAndInvalidateCache_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Name = "Old Name",
            Email = "old@gamestore.com",
            Password = "old_hashed_password"
        };

        var request = new UpdateUserRequest
        {
            Name = "New Name",
            Email = "new@gamestore.com",
            Password = "NewPassword@123"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

        var result = await _service.UpdateAsync(userId, request);

        Assert.Equal("New Name", result.Name);
        Assert.Equal("new@gamestore.com", result.Email);
        Assert.NotEqual("NewPassword@123", result.Password);

        _repositoryMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("users:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            Name = "New Name",
            Email = "new@gamestore.com",
            Password = "NewPassword@123"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(userId, request));

        Assert.Equal("User not found.", exception.Message);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}