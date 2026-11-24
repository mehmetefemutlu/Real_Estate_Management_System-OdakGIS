namespace srs.DTO
{
    public class PolygonDTO
    {
        public int id { get; set; }
        public int ownerId { get; set; }
        public string label { get; set; } = string.Empty;
        public string coordinates { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    public class CreatePolygonDTO
    {
        public string label { get; set; } = string.Empty;
        public string coordinates { get; set; } = string.Empty;
    }

    public class UpsertPolygonsDTO
    {
        public List<CreatePolygonDTO> polygons { get; set; } = [];
    }

    public class CreatePolygonsDTO
    {
        public List<CreatePolygonDTO> polygons { get; set; } = [];
    }
}
