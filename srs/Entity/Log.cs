namespace srs.Entity
{
    public class Log
    {

        public int id { get; set; }
        public int userId { get; set; }
        public User? user { get; set; }
        public OperationType operationType { get; set; }
        public string message { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Status status { get; set; }
        public string userIP { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public enum OperationType
        {
            Create = 1,
            Update = 2,
            Delete = 3,
            Read = 4
        }
        public enum Status
        {
            Active = 1,
            Pending = 2,
            Completed = 3,
            Aborted = 4
        }
    }
}
