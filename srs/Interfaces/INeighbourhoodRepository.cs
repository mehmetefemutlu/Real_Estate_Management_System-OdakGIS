using srs.Entity;

namespace srs.Interfaces
{
    public interface INeighbourhoodRepository : IRepository<Neighbourhood>
    {
        Task<IEnumerable<Neighbourhood>> GetAllWithDistrict();
        Task<IEnumerable<Neighbourhood>> GetAllWithPropertiesAndDistrict();
        Task<Neighbourhood?> GetNeighbourhoodWithProperties(int id);
        Task<Neighbourhood?> GetByLocationNames(string cityName, string districtName, string neighbourhoodName);
        Task<IEnumerable<Neighbourhood>> GetNeighbourhoodsByDistrict(int districtId);
        Task<bool> NeighbourhoodExists(int neighbourhoodId);
    }
}
