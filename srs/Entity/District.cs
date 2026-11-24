namespace srs.Entity
{
    public class District
    {
        public int id { get; set; }
        public string districtName { get; set; } = string.Empty;
        public int cityId { get; set; }
        public City city { get; set; } = null!;
        public string postalCode { get; set; } = string.Empty;
        public ICollection<Neighbourhood> neighbourhoods { get; set; } = new List<Neighbourhood>();
    }
}
