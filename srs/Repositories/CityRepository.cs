using Microsoft.EntityFrameworkCore;
using srs.Data;
using srs.Entity;
using srs.Interfaces;

namespace srs.Repositories
{
    public class CityRepository : Repository<City>, ICityRepository
    {
        public CityRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<City?> GetCityWithDistricts(int id)
        {
            return await _dbSet
                .Include(c => c.districts)
                    .ThenInclude(d => d.neighbourhoods)
                .FirstOrDefaultAsync(c => c.id == id);
        }

        public async Task<IEnumerable<City>> GetCitiesWithDistricts()
        {
            return await _dbSet
                .Include(c => c.districts)
                    .ThenInclude(d => d.neighbourhoods)
                .ToListAsync();
        }

        public async Task<bool> CityExistsByName(string cityName)
        {
            return await _dbSet.AnyAsync(c => c.cityName == cityName);
        }
    }
}
