using srs.Entity;

namespace srs.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserWithProperties(int id);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByNameOrEmail(string ownerName);
        Task<bool> EmailExists(string email);
        Task<bool> ValidateUserCredentials(string email, string passwordHash);
        Task<string> GetUserNameByIdAsync(int id);
        Task DeleteUserAndProperties(int id);
    }
}
