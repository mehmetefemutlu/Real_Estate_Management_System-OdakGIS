using srs.Entity;

namespace srs.DTO
{
    public class LogDTO
    {
        public int id { get; set; }
        public int userId { get; set; }
        public string userName { get; set; } = string.Empty;
        public Log.OperationType operationType { get; set; }
        public string message { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Log.Status status { get; set; }
        public string userIP { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
    }
    public class LogViewDTO
    {
        public int id { get; set; }
        public string userName { get; set; } = string.Empty;
        public int userId { get; set; }
        public Log.OperationType operationType { get; set; }
        public string message { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Log.Status status { get; set; }
        public string userIP { get; set; } = string.Empty;
        public DateTime timestamp { get; set; }
    }
    public class CreateLogDTO
    {
        public int userId { get; set; }
        public Log.OperationType operationType { get; set; }
        public string message { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Log.Status status { get; set; }
        public string userIP { get; set; } = string.Empty;
    }
    public class FilterLogDTO
    {
        public int? userId { get; set; }
        public Log.OperationType? operationType { get; set; }
        public Log.Status? status { get; set; }
        public string? description { get; set; }
        public string? userIP { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }
}
