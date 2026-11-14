namespace UsersAPI.DTOs;

public record LoginRequest(
    string Email,
    string Password
);

