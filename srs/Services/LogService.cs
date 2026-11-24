using srs.DTO;
using srs.Entity;
using srs.Interfaces;

namespace srs.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;
        private readonly IUserRepository _userRepository;

        public LogService(ILogRepository logRepository, IUserRepository userRepository)
        {
            _logRepository = logRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<LogDTO>> GetAllLogsAsync()
        {
            var logs = await _logRepository.GetAll();
            var logDtos = new List<LogDTO>();
            foreach (var log in logs)
            {
                logDtos.Add(await MapToLogDTO(log));
            }
            return logDtos;
        }

        public async Task<IEnumerable<LogViewDTO>> GetAllLogsViewAsync()
        {
            var logs = await _logRepository.GetAll();
            var logViewDtos = new List<LogViewDTO>();
            foreach (var log in logs)
            {
                var userName = await _userRepository.GetUserNameByIdAsync(log.userId);
                logViewDtos.Add(new LogViewDTO
                {
                    id = log.id,
                    userName = userName,
                    userId = log.userId,
                    operationType = log.operationType,
                    message = log.message,
                    description = log.description,
                    status = log.status,
                    userIP = log.userIP,
                    timestamp = log.createdAt
                });
            }
            return logViewDtos;
        }

        public async Task<LogDTO?> GetLogByIdAsync(int id)
        {
            var log = await _logRepository.GetById(id);
            if (log == null) return null;

            return await MapToLogDTO(log);
        }

        public async Task<IEnumerable<LogDTO>> GetLogsByUserAsync(int userId)
        {
            var logs = await _logRepository.GetLogsByUser(userId);
            var logDtos = new List<LogDTO>();
            foreach (var log in logs)
            {
                logDtos.Add(await MapToLogDTO(log));
            }
            return logDtos;
        }

        public async Task<IEnumerable<LogDTO>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var logs = await _logRepository.GetLogsByDateRange(startDate, endDate);
            var logDtos = new List<LogDTO>();
            foreach (var log in logs)
            {
                logDtos.Add(await MapToLogDTO(log));
            }
            return logDtos;
        }

        public async Task<IEnumerable<LogDTO>> GetFilteredLogsAsync(FilterLogDTO filterDto)
        {
            var logs = await _logRepository.GetFilteredLogs(
                filterDto.userId,
                filterDto.startDate,
                filterDto.endDate,
                filterDto.operationType,
                filterDto.status,
                filterDto.description,
                filterDto.userIP,
                filterDto.page,
                filterDto.pageSize
            );

            var logDtos = new List<LogDTO>();
            foreach (var log in logs)
            {
                logDtos.Add(await MapToLogDTO(log));
            }
            return logDtos;
        }

        public async Task<LogDTO> CreateLogAsync(CreateLogDTO createLogDto)
        {
            var userExists = await _userRepository.Exists(createLogDto.userId);
            if (!userExists)
                throw new KeyNotFoundException($"User with id {createLogDto.userId} not found");

            var log = new Log
            {
                userId = createLogDto.userId,
                operationType = (Log.OperationType)createLogDto.operationType,
                message = createLogDto.message,
                description = createLogDto.description,
                status = (Log.Status)createLogDto.status,
                userIP = createLogDto.userIP,
                createdAt = DateTime.UtcNow
            };

            var createdLog = await _logRepository.Add(log);
            return await MapToLogDTO(createdLog);
        }

        public async Task DeleteLogAsync(int id)
        {
            var exists = await _logRepository.Exists(id);
            if (!exists)
                throw new KeyNotFoundException($"Log with id {id} not found");

            await _logRepository.Delete(id);
        }

        private async Task<LogDTO> MapToLogDTO(Log log)
        {
            return new LogDTO
            {
                id = log.id,
                userId = log.userId,
                userName = await _userRepository.GetUserNameByIdAsync(log.userId),
                operationType = log.operationType,
                message = log.message,
                description = log.description,
                status = log.status,
                userIP = log.userIP,
                createdAt = log.createdAt
            };
        }
    }
}
