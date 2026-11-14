namespace UsersAPI.DTOs;

public record UpdateUserByAdminRequest(
    string Name,
    string Email,
    string Role
);

