namespace srs.Entity
{
    public class City
    {
        public int id { get; set; }
        public string cityName { get; set; } = string.Empty;
        public ICollection<District> districts { get; set; } = new List<District>();
    }
}
