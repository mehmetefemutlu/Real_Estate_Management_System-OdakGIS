namespace srs.Entity
{
    public class Property
    {
        public int id { get; set; }
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string address { get; set; } = string.Empty;
        public bool isReadOnly { get; set; }
        public int ownerId { get; set; }
        public User owner { get; set; } = null!;
        public string parcelNumber { get; set; } = string.Empty;
        public string lotNumber { get; set; } = string.Empty;
        public double area { get; set; }
        public string coordinates { get; set; } = string.Empty;
        public string imagePath { get; set; } = string.Empty;
        public int bedrooms { get; set; }
        public int bathrooms { get; set; }
        public int parkingSpaces { get; set; }
        public PropertyType type { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int neighbourhoodId { get; set; }
        public Neighbourhood neighbourhood { get; set; } = null!;
 
        public enum PropertyType
        {
            Apartment = 0,
            House = 1,
            Condo = 2,
            Townhouse = 3,
            Land = 4
        }
    }
}
