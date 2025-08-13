using Authorization.Domain.Entities;

namespace Authorization.Application.interfaces;

public interface IUserService
{
    Task<User?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<bool> IsLockedOutAsync(User user);
}