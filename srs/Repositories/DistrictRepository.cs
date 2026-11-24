using Microsoft.EntityFrameworkCore;
using srs.Data;
using srs.Entity;
using srs.Interfaces;

namespace srs.Repositories
{
    public class DistrictRepository : Repository<District>, IDistrictRepository
    {
        public DistrictRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<District>> GetAllWithCity()
        {
            return await _dbSet
                .Include(d => d.city)
                .ToListAsync();
        }

        public async Task<District?> GetDistrictWithNeighbourhoods(int id)
        {
            return await _dbSet
                .Include(d => d.neighbourhoods)
                .Include(d => d.city)
                .FirstOrDefaultAsync(d => d.id == id);
        }

        public async Task<IEnumerable<District>> GetDistrictsByCity(int cityId)
        {
            return await _dbSet
                .Where(d => d.cityId == cityId)
                .ToListAsync();
        }

        public async Task<bool> DistrictExists(int districtId)
        {
            return await _dbSet.AnyAsync(d => d.id == districtId);
        }
    }
}
