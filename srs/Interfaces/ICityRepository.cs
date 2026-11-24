using srs.Entity;

namespace srs.Interfaces
{
    public interface ICityRepository : IRepository<City>
    {
        Task<City?> GetCityWithDistricts(int id);
        Task<IEnumerable<City>> GetCitiesWithDistricts();
        Task<bool> CityExistsByName(string cityName);
    }
}
