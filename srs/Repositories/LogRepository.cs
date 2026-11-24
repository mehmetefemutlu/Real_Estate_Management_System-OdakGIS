using Microsoft.EntityFrameworkCore;
using srs.Data;
using srs.Entity;
using srs.Interfaces;

namespace srs.Repositories
{
    public class LogRepository : Repository<Log>, ILogRepository
    {
        public LogRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Log>> GetLogsByUser(int userId)
        {
            return await _dbSet
                .Where(l => l.userId == userId)
                .OrderByDescending(l => l.createdAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Log>> GetLogsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(l => l.createdAt >= startDate && l.createdAt <= endDate)
                .OrderByDescending(l => l.createdAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Log>> GetLogsByOperationType(Log.OperationType operationType)
        {
            return await _dbSet
                .Where(l => l.operationType == operationType)
                .OrderByDescending(l => l.createdAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Log>> GetLogsByStatus(Log.Status status)
        {
            return await _dbSet
                .Where(l => l.status == status)
                .OrderByDescending(l => l.createdAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Log>> GetFilteredLogs(
            int? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Log.OperationType? operationType = null,
            Log.Status? status = null,
            string? description = null,
            string? userIP = null,
            int pageNumber = 1,
            int pageSize = 50)
        {
            var query = _dbSet.AsQueryable();

            if (userId.HasValue)
                query = query.Where(l => l.userId == userId.Value);

            if (startDate.HasValue)
                query = query.Where(l => l.createdAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.createdAt <= endDate.Value);

            if (operationType.HasValue)
                query = query.Where(l => l.operationType == operationType.Value);

            if (status.HasValue)
                query = query.Where(l => l.status == status.Value);

            if (!string.IsNullOrWhiteSpace(description))
                query = query.Where(l => l.description.ToLower().Contains(description.Trim().ToLower()));

            if (!string.IsNullOrWhiteSpace(userIP))
                query = query.Where(l => l.userIP.ToLower().Contains(userIP.Trim().ToLower()));

            return await query
                .OrderByDescending(l => l.createdAt)
                .Skip((Math.Max(pageNumber, 1) - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
