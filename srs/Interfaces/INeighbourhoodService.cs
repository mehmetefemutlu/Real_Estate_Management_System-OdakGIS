using srs.DTO;

namespace srs.Interfaces
{
    public interface INeighbourhoodService
    {
        Task<IEnumerable<NeighbourhoodDTO>> GetAllNeighbourhoodsAsync();
        Task<IEnumerable<NeighbourhoodViewDTO>> GetAllNeighbourhoodsViewAsync();
        Task<NeighbourhoodDTO?> GetNeighbourhoodByIdAsync(int id);
        Task<IEnumerable<NeighbourhoodViewDTO>> GetNeighbourhoodsByDistrictAsync(int districtId);
        Task<NeighbourhoodDTO> CreateNeighbourhoodAsync(CreateNeighbourhoodDTO createNeighbourhoodDto);
        Task UpdateNeighbourhoodAsync(int id, UpdateNeighbourhoodDTO updateNeighbourhoodDto);
        Task DeleteNeighbourhoodAsync(int id);
        Task<bool> NeighbourhoodExistsAsync(int id);
    }
}
