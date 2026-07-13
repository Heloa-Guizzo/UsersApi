using Microsoft.AspNetCore.Mvc;
using UsersAPI.DTOs;
using UsersAPI.Services;
using Microsoft.AspNetCore.Authorization;
using MassTransit;
using Shared.Events;

namespace UsersAPI.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly IPublishEndpoint _publish;

    public AuthController(UserService userService, AuthService authService, JwtService jwtService, IPublishEndpoint publish)
    {
        _userService = userService;
        _authService = authService;
        _jwtService = jwtService;
        _publish = publish; 
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var user = await _userService.RegisterAsync(request);

        Console.WriteLine($"PUBLICANDO EVENTO: {user.Email}");

        await _publish.Publish(
            new UserCreatedEvent(
                user.Id,
                user.Email));

        Console.WriteLine("EVENTO PUBLICADO");

        return Created("", new
        {
            user.Id,
            user.Name,
            user.Email
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userService.GetByEmailAsync(request.Email);


        if (user == null || user.Password != request.Password)
            return Unauthorized();

        var token = _jwtService.GenerateToken(user);

        return Ok(new { token });
    }
}

