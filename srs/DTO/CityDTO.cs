namespace srs.DTO
{
    public class CityDTO
    {
        public int id { get; set; }
        public string cityName { get; set; } = string.Empty;
        public List<DistrictDTO>? districts { get; set; }
    }
    public class CityViewDTO
    {
        public int id { get; set; }
        public string cityName { get; set; } = string.Empty;
    }
    public class CreateCityDTO
    {
        public string cityName { get; set; } = string.Empty;
    }
    public class UpdateCityDTO
    {
        public string cityName { get; set; } = string.Empty;
    }
}
