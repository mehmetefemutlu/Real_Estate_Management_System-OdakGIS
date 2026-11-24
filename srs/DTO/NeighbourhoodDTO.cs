namespace srs.DTO
{
    public class NeighbourhoodDTO
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int districtId { get; set; }
        public string? districtName { get; set; }
        public string? postalCode { get; set; }
        public string? cityName { get; set; }
        public List<PropertyViewDTO>? properties { get; set; }
    }
    public class NeighbourhoodViewDTO
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string districtName { get; set; } = string.Empty;
        public string cityName { get; set; } = string.Empty;    
    }
    public class CreateNeighbourhoodDTO
    {
        public string name { get; set; } = string.Empty;
        public int districtId { get; set; }
    }
    public class UpdateNeighbourhoodDTO
    {
        public string? name { get; set; }
        public int? districtId { get; set; }
    }
}
