using srs.DTO;

namespace srs.Interfaces
{
    public interface IDistrictService
    {
        Task<IEnumerable<DistrictDTO>> GetAllDistrictsAsync();
        Task<IEnumerable<DistrictViewDTO>> GetAllDistrictsViewAsync();
        Task<DistrictDTO?> GetDistrictByIdAsync(int id);
        Task<IEnumerable<DistrictViewDTO>> GetDistrictsByCityAsync(int cityId);
        Task<DistrictDTO> CreateDistrictAsync(CreateDistrictDTO createDistrictDto);
        Task UpdateDistrictAsync(int id, UpdateDistrictDTO updateDistrictDto);
        Task DeleteDistrictAsync(int id);
        Task<bool> DistrictExistsAsync(int id);
    }
}
