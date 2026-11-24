using srs.Entity;

namespace srs.Interfaces
{
    public interface IDistrictRepository : IRepository<District>
    {
        Task<IEnumerable<District>> GetAllWithCity();
        Task<District?> GetDistrictWithNeighbourhoods(int id);
        Task<IEnumerable<District>> GetDistrictsByCity(int cityId);
        Task<bool> DistrictExists(int districtId);
    }
}
