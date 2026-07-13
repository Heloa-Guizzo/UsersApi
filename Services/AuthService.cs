using UsersAPI.DTOs;
using UsersAPI.Exceptions;
using UsersAPI.Helpers;
using UsersAPI.Models;

namespace UsersAPI.Services;

public class AuthService
{
    private readonly UserService _userService;

    public AuthService(UserService userService)
    {
        _userService = userService;
    }

    public async Task<User> LoginAsync(LoginRequest request)
    {
        var user =
            await _userService.GetByEmailAsync(
                request.Email);

        if (user == null)
            throw new UnauthorizedException(
                "Invalid credentials.");

        var validPassword =
            PasswordHasher.Verify(
                request.Password,
                user.Password);

        if (!validPassword)
            throw new UnauthorizedException(
                "Invalid credentials.");

        return user;
    }
}
