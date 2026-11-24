using srs.DTO;

namespace srs.Interfaces
{
    public interface IPropertyService
    {
        Task<IEnumerable<PropertyDTO>> GetAllPropertiesAsync();
        Task<IEnumerable<PropertyViewDTO>> GetAllPropertiesViewAsync();
        Task<PropertyDTO?> GetPropertyByIdAsync(int id);
        Task<IEnumerable<PropertyViewDTO>> GetPropertiesByUserAsync(int ownerId);
        Task<IEnumerable<PropertyViewDTO>> GetPropertiesByNeighbourhoodAsync(int neighbourhoodId);
        Task<IEnumerable<PropertyViewDTO>> GetPropertiesByDistrictAsync(int districtId);
        Task<IEnumerable<PropertyViewDTO>> GetPropertiesByCityAsync(int cityId);
        Task<IEnumerable<PropertyDTO>> SearchPropertiesAsync(SearchPropertyDTO searchDto);
        Task<PropertyDTO> CreatePropertyAsync(CreatePropertyDTO createPropertyDto, string userIP = "");
        Task UpdatePropertyAsync(int id, UpdatePropertyDTO updatePropertyDto, string userIP = "");
        Task DeletePropertyAsync(int id, string userIP = "");
        Task DeletePropertiesAsync(IEnumerable<int> ids, string userIP = "");
        Task<bool> PropertyExistsAsync(int id);
        Task<int> GetPropertyCountByUserAsync(int ownerId);
    }
}
