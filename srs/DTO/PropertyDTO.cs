using srs.Entity;

namespace srs.DTO
{
    public class PropertyDTO
    {
        public int propertyID { get; set; }
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string address { get; set; } = string.Empty;
        public bool isReadOnly { get; set; }
        public int ownerId { get; set; }
        public string ownerName { get; set; } = string.Empty;
        public string ownerEmail { get; set; } = string.Empty;
        public string parcelNumber { get; set; } = string.Empty;    
        public string lotNumber { get; set; } = string.Empty;  
        public double area { get; set; }
        public string coordinates { get; set; } = string.Empty;
        public string imagePath { get; set; } = string.Empty;
        public int bedrooms { get; set; }
        public int bathrooms { get; set; }
        public int parkingSpaces { get; set; }
        public Property.PropertyType propertyType { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int neighbourhoodId { get; set; }
        public string neighbourhoodName { get; set; } = string.Empty;
        public string districtName { get; set; } = string.Empty;
        public string cityName { get; set; } = string.Empty;
    }
    public class PropertyViewDTO
    {
        public int propertyID { get; set; }
        public string title { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string address { get; set; } = string.Empty;
        public double area { get; set; }
        public string coordinates { get; set; } = string.Empty;
        public string parcelNumber { get; set; } = string.Empty;
        public string lotNumber { get; set; } = string.Empty;
        public Property.PropertyType propertyType { get; set; }
        public bool isReadOnly { get; set; }
        public string districtName { get; set; } = string.Empty;
        public string cityName { get; set; } = string.Empty;
        public int ownerId { get; set; }
        public string ownerName { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
    }
    public class CreatePropertyDTO
    {
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public decimal price { get; set; }
        public string address { get; set; } = string.Empty;
        public bool isReadOnly { get; set; }
        public int ownerId { get; set; }
        public string parcelNumber { get; set; } = string.Empty;
        public string lotNumber { get; set; } = string.Empty;
        public double area { get; set; }
        public string coordinates { get; set; } = string.Empty;
        public string imagePath { get; set; } = string.Empty;
        public int bedrooms { get; set; }
        public int bathrooms { get; set; }
        public int parkingSpaces { get; set; }
        public Property.PropertyType propertyType { get; set; }
        public int neighbourhoodId { get; set; }
    }
    public class UpdatePropertyDTO
    {
        public string? title { get; set; }
        public string? description { get; set; }
        public decimal? price { get; set; }
        public string? address { get; set; }
        public bool? isReadOnly { get; set; }
        public string? parcelNumber { get; set; }
        public string? lotNumber { get; set; }
        public double? area { get; set; }
        public string? coordinates { get; set; }
        public string? imagePath { get; set; }
        public int? bedrooms { get; set; }
        public int? bathrooms { get; set; }
        public int? parkingSpaces { get; set; }
        public Property.PropertyType propertyType { get; set; }
        public int? neighbourhoodId { get; set; }
    }
    public class SearchPropertyDTO
    {
        public decimal? minPrice { get; set; }
        public decimal? maxPrice { get; set; }
        public int? minBedrooms { get; set; }
        public int? maxBedrooms { get; set; }
        public double? minArea { get; set; }
        public double? maxArea { get; set; }
        public Property.PropertyType? propertyType { get; set; }
        public int? cityId { get; set; }
        public int? districtId { get; set; }
        public int? neighbourhoodId { get; set; }
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
        public string parcelNumber { get; set; } = string.Empty;
        public string lotNumber { get; set; } = string.Empty;
        public int? ownerId { get; set; }
    }

    public class BulkDeletePropertyDTO
    {
        public List<int> propertyIds { get; set; } = new();
    }
}
