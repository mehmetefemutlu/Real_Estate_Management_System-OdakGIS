namespace srs.Entity
{
    public class User
    {
        public int id { get; set; }
        public string firstname { get; set; } = string.Empty;
        public string lastname { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string passwordHash { get; set; } = string.Empty;
        public string passwordSalt { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public Role userRole { get; set; }
        public ICollection<Property> properties { get; set; } = new List<Property>();
        public ICollection<Polygon> polygons { get; set; } = new List<Polygon>();
        public ICollection<Log> logs { get; set; } = new List<Log>();
        public enum Role
        {
            Admin = 0,
            User = 1, 
            Guest = 2,
        }
    }
}
