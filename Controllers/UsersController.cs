using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersAPI.DTOs;
using UsersAPI.Services;
using Shared.Events;

namespace UsersAPI.Controllers;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IPublishEndpoint _publish;

    public UsersController(UserService userService, IPublishEndpoint publish)
    {
        _userService = userService;
        _publish = publish;
    }


    [HttpPost]
    public async Task<IActionResult> Create(RegisterUserRequest request)
    {
        var user = await _userService.RegisterAsync(request);

        await _publish.Publish(new UserCreatedEvent(user.Id, user.Email));

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email
        });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateAsync(id, request);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email
        });
    }
}
