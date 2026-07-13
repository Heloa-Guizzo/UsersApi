using UsersAPI.Exceptions;
using UsersAPI.Helpers;
using UsersAPI.Models;
using UsersAPI.Repositories;
using UsersAPI.ValueObjects;

namespace UsersAPI.Services
{
    public class AdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(IUserRepository userRepository, ILogger<AdminUserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }
        public async Task<User> RegisterAsync(
            string name,
            string email,
            string password)
        {
            PasswordValidator.Validate(password);

            var passwordHash = PasswordHasher.Hash(password);
            var emailVo = Email.Create(email);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = emailVo.Value,
                Password = passwordHash,
                Role = "User"
            };


            await _userRepository.AddAsync(user);

            _logger.LogInformation("Admin created user | UserId={UserId} | Email={Email}", user.Id, user.Email);

            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            _logger.LogInformation("Admin requested user list");

            return await _userRepository.GetAllAsync();
        }
        public async Task<User?> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Admin requested user details | UserId={UserId}", id);

            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> UpdateByAdminAsync(Guid userId, string role)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
            {
                _logger.LogWarning(
                    "Admin attempted to update role of non-existing user | UserId={UserId}",
                    userId
                );
                throw new NotFoundException("User not found.");
            }

            if (role != "User" && role != "Admin")
                throw new ArgumentException("Invalid role.");

            user.Role = role;

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation(
                "Admin updated user role | UserId={UserId} | NewRole={Role}",
                user.Id,
                role
            );

            return user;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
            {
                _logger.LogWarning("Admin attempted to delete non-existing user | UserId={UserId}", id);

                throw new NotFoundException("User not found.");
            }

            await _userRepository.DeleteAsync(user);

            _logger.LogInformation("Admin deleted user | UserId={UserId} | Email={Email}", user.Id, user.Email);
        }
    }
}
