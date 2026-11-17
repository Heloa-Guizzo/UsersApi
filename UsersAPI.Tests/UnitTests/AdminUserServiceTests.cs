using Microsoft.Extensions.Logging;
using Moq;
using UsersAPI.Exceptions;
using UsersAPI.Models;
using UsersAPI.Repositories;
using UsersAPI.Services;
using Xunit;

namespace UsersAPI.Tests;

public class AdminUserServiceTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly Mock<ILogger<AdminUserService>> _loggerMock;
    private readonly AdminUserService _service;

    public AdminUserServiceTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<AdminUserService>>();
        _service = new AdminUserService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserWithHashedPasswordAndDefaultRole_WhenValid()
    {
        var result = await _service.RegisterAsync("Admin Created", "novo@gamestore.com", "Password@123");

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Admin Created", result.Name);
        Assert.Equal("novo@gamestore.com", result.Email);
        Assert.Equal("User", result.Role);
        Assert.NotEqual("Password@123", result.Password);

        _repositoryMock.Verify(r => r.AddAsync(It.Is<User>(u => u.Email == "novo@gamestore.com")), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsersFromRepository()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "User 1", Email = "u1@gamestore.com", Role = "User" },
            new() { Id = Guid.NewGuid(), Name = "Admin 1", Email = "a1@gamestore.com", Role = "Admin" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Target User", Email = "target@gamestore.com", Role = "User" };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _service.GetByIdAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("User")]
    public async Task UpdateByAdminAsync_ShouldUpdateRole_WhenUserExistsAndRoleIsValid(string newRole)
    {
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Name = "Player", Email = "player@gamestore.com", Role = "User" };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

        var result = await _service.UpdateByAdminAsync(userId, newRole);

        Assert.Equal(newRole, result.Role);
        _repositoryMock.Verify(r => r.UpdateAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task UpdateByAdminAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateByAdminAsync(userId, "Admin"));

        Assert.Equal("User not found.", exception.Message);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateByAdminAsync_ShouldThrowArgumentException_WhenRoleIsInvalid()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Name = "Player", Email = "player@gamestore.com", Role = "User" };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateByAdminAsync(userId, "SuperAdmin"));

        Assert.Equal("Invalid role.", exception.Message);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUser_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User { Id = userId, Name = "To Delete", Email = "del@gamestore.com", Role = "User" };

        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);

        await _service.DeleteUserAsync(userId);

        _repositoryMock.Verify(r => r.DeleteAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteUserAsync(userId));

        Assert.Equal("User not found.", exception.Message);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<User>()), Times.Never);
    }
}