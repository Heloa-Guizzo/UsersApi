using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UsersAPI.DTOs;
using UsersAPI.Exceptions;
using UsersAPI.Helpers;
using UsersAPI.Models;
using UsersAPI.Repositories;
using UsersAPI.ValueObjects;

namespace UsersAPI.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserService> _logger;
    private const string AllUsersCacheKey = "users:all";

    public UserService(
        IUserRepository userRepository,
        IDistributedCache cache,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(RegisterUserRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
            throw new ConflictException("Email already registered.");

        PasswordValidator.Validate(request.Password);
        var email = Email.Create(request.Email);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = email.Value,
            Password = PasswordHasher.Hash(request.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user);
        await _cache.RemoveAsync(AllUsersCacheKey);

        _logger.LogInformation("User created successfully. UserId: {UserId}", user.Id);
        return user;
    }

    public async Task<(IEnumerable<User> Users, bool IsCacheHit)> GetAllWithCacheInfoAsync()
    {
        var cachedData = await _cache.GetStringAsync(AllUsersCacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedUsers = JsonSerializer.Deserialize<List<User>>(cachedData) ?? [];
            return (cachedUsers, true);
        }

        var users = (await _userRepository.GetAllAsync()).ToList();

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(AllUsersCacheKey, JsonSerializer.Serialize(users), options);

        return (users, false);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var (users, _) = await GetAllWithCacheInfoAsync();
        return users;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException("User not found.");

        PasswordValidator.Validate(request.Password);
        var email = Email.Create(request.Email);

        user.Name = request.Name;
        user.Email = email.Value;
        user.Password = PasswordHasher.Hash(request.Password);

        await _userRepository.UpdateAsync(user);
        await _cache.RemoveAsync(AllUsersCacheKey);

        _logger.LogInformation("User updated successfully. UserId: {UserId}", user.Id);
        return user;
    }
}