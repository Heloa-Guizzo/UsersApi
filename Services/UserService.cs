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
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> RegisterAsync(
        RegisterUserRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new ConflictException(
                "Email already registered.");
        }

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

        _logger.LogInformation(
            "User created successfully. UserId: {UserId}",
            user.Id);

        return user;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }


    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User> UpdateAsync(
        Guid id,
        UpdateUserRequest request)
    {
        var user =
            await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        PasswordValidator.Validate(
            request.Password);

        var email = Email.Create(
            request.Email);

        user.Name = request.Name;
        user.Email = email.Value;
        user.Password =
            PasswordHasher.Hash(
                request.Password);

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation(
            "User updated successfully. UserId: {UserId}",
            user.Id);

        return user;
    }
}