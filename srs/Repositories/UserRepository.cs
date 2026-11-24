using Microsoft.EntityFrameworkCore;
using srs.Data;
using srs.Entity;
using srs.Interfaces;

namespace srs.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetUserWithProperties(int id)
        {
            return await _dbSet
                .Include(u => u.properties)
                .FirstOrDefaultAsync(u => u.id == id);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.email == email);
        }

        public async Task<User?> GetUserByNameOrEmail(string ownerName)
        {
            var normalized = NormalizeOwnerName(ownerName);
            var users = await _dbSet.ToListAsync();

            return users.FirstOrDefault(u =>
                NormalizeOwnerName(u.email) == normalized ||
                NormalizeOwnerName($"{u.firstname} {u.lastname}") == normalized);
        }

        public async Task<bool> EmailExists(string email)
        {
            return await _dbSet.AnyAsync(u => u.email == email);
        }

        public async Task<bool> ValidateUserCredentials(string email, string passwordHash)
        {
            return await _dbSet.AnyAsync(u => u.email == email && u.passwordHash == passwordHash);
        }

        public async Task<string> GetUserNameByIdAsync(int id)
        {
            var user = await _dbSet
                .FirstOrDefaultAsync(u => u.id == id);
            return user != null ? $"{user.firstname} {user.lastname}" : string.Empty;
        }

        public async Task DeleteUserAndProperties(int id)
        {
            var user = await _dbSet
                .Include(u => u.properties)
                .FirstOrDefaultAsync(u => u.id == id);

            if (user == null)
                return;

            if (user.properties.Count > 0)
            {
                _context.properties.RemoveRange(user.properties);
            }

            _dbSet.Remove(user);
            await _context.SaveChangesAsync();
        }

        private static string NormalizeOwnerName(string value)
        {
            return string.Join(' ', value
                .Trim()
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
