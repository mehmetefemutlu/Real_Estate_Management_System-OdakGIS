namespace srs.Entity
{
    public class Neighbourhood
    {

        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int districtId { get; set; }
        public District district { get; set; } = null!;
        public ICollection<Property> properties { get; set; } = new List<Property>();
    }
}
