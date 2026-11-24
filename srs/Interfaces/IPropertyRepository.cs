using srs.Entity;

namespace srs.Interfaces
{
    public interface IPropertyRepository : IRepository<Property>
    {
        Task<Property?> GetPropertyWithDetails(int id);
        Task<IEnumerable<Property>> GetPropertiesByUser(int ownerId);
        Task<IEnumerable<Property>> GetPropertiesByNeighbourhood(int neighbourhoodId);
        Task<IEnumerable<Property>> GetPropertiesByDistrict(int districtId);
        Task<IEnumerable<Property>> GetPropertiesByCity(int cityId);
        Task<IEnumerable<Property>> SearchProperties(
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? minBedrooms = null,
            int? maxBedrooms = null,
            double? minArea = null,
            double? maxArea = null,
            Property.PropertyType? propertyType = null,
            int? cityId = null,
            int? districtId = null,
            int? neighbourhoodId = null,
            string? parcelNumber = null,
            string? lotNumber = null,
            int? ownerId = null);
        Task<int> GetPropertyCountByUser(int ownerId);
    }
}
