namespace srs.DTO
{
    public class DistrictDTO
    {
        public int id { get; set; }
        public string districtName { get; set; } = string.Empty;
        public int cityId { get; set; }
        public string cityName { get; set; } = string.Empty;
        public string postalCode { get; set; } = string.Empty;
        public List<NeighbourhoodViewDTO>? neighbourhoods { get; set; }
    }
    public class DistrictViewDTO
    {
        public int id { get; set; }
        public string districtName { get; set; } = string.Empty;
        public string cityName { get; set; } = string.Empty;
        public string postalCode { get; set; } = string.Empty;
    }
    public class CreateDistrictDTO
    {
        public string districtName { get; set; } = string.Empty;
        public int cityId { get; set; }
        public string postalCode { get; set; } = string.Empty;
    }
    public class UpdateDistrictDTO
    {
        public string? districtName { get; set; }
        public int? cityId { get; set; }
        public string? postalCode { get; set; }
    }
}
