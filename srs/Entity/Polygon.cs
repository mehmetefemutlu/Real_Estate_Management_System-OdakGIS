namespace srs.Entity
{
    public class Polygon
    {
        public int id { get; set; }
        public int ownerId { get; set; }
        public User? owner { get; set; }
        public string label { get; set; } = string.Empty;
        public string coordinates { get; set; } = string.Empty;
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    }
}
