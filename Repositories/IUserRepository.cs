using UsersAPI.Models;

namespace UsersAPI.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(Guid id);

    Task<IEnumerable<User>> GetAllAsync();

    Task<bool> ExistsByEmailAsync(string email);

    Task UpdateAsync(User user);

    Task DeleteAsync(User user);
}