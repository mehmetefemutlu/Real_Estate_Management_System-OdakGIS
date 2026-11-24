using srs.Entity;

namespace srs.Interfaces
{
    public interface ILogRepository : IRepository<Log>
    {
        Task<IEnumerable<Log>> GetLogsByUser(int userId);
        Task<IEnumerable<Log>> GetLogsByDateRange(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Log>> GetLogsByOperationType(Log.OperationType operationType);
        Task<IEnumerable<Log>> GetLogsByStatus(Log.Status status);
        Task<IEnumerable<Log>> GetFilteredLogs(
            int? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            Log.OperationType? operationType = null,
            Log.Status? status = null,
            string? description = null,
            string? userIP = null,
            int pageNumber = 1,
            int pageSize = 50);
    }
}
