using srs.DTO;

namespace srs.Interfaces
{
    public interface ILogService
    {
        Task<IEnumerable<LogDTO>> GetAllLogsAsync();
        Task<IEnumerable<LogViewDTO>> GetAllLogsViewAsync();
        Task<LogDTO?> GetLogByIdAsync(int id);
        Task<IEnumerable<LogDTO>> GetLogsByUserAsync(int userId);
        Task<IEnumerable<LogDTO>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<LogDTO>> GetFilteredLogsAsync(FilterLogDTO filterDto);
        Task<LogDTO> CreateLogAsync(CreateLogDTO createLogDto);
        Task DeleteLogAsync(int id);
    }
}
