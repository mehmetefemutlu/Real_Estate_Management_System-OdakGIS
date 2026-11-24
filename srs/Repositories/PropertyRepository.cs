using Microsoft.EntityFrameworkCore;
using srs.Data;
using srs.Entity;
using srs.Interfaces;

namespace srs.Repositories
{
    public class PropertyRepository : Repository<Property>, IPropertyRepository
    {
        public PropertyRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Property>> GetAll()
        {
            return await _dbSet
                .Include(p => p.owner)
                .Include(p => p.neighbourhood)
                    .ThenInclude(n => n.district)
                        .ThenInclude(d => d.city)
                .ToListAsync();
        }

        public async Task<Property?> GetPropertyWithDetails(int id)
        {
            return await _dbSet
                .Include(p => p.neighbourhood)
                    .ThenInclude(n => n.district)
                        .ThenInclude(d => d.city)
                .Include(p => p.owner)
                .FirstOrDefaultAsync(p => p.id == id);
        }

        public async Task<IEnumerable<Property>> GetPropertiesByUser(int ownerId)
        {
            return await _dbSet
                .Include(p => p.neighbourhood)
                    .ThenInclude(n => n.district)
                        .ThenInclude(d => d.city)
                .Where(p => p.ownerId == ownerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetPropertiesByNeighbourhood(int neighbourhoodId)
        {
            return await _dbSet
                .Include(p => p.owner)
                .Include(p => p.neighbourhood)
                    .ThenInclude(n => n.district)
                        .ThenInclude(d => d.city)
                .Where(p => p.neighbourhoodId == neighbourhoodId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetPropertiesByDistrict(int districtId)
        {
            return await _dbSet
                .Include(p => p.neighbourhood)
                    .ThenInclude(n => n.district)
                        .ThenInclude(d => d.city)
                .Where(p => p.neighbourhood.districtId == districtId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetPropertiesByCity(int cityId)
        {
            return await _dbSet
                .Include(p => p.neighbourhood)
                    .ThenInclude(n => n.district)
                        .ThenInclude(d => d.city)
                .Where(p => p.neighbourhood.district.cityId == cityId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> SearchProperties(
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
            int? ownerId = null)
        {
            var query = _dbSet
                .Include(p => p.owner)
                .Include(p => p.neighbourhood)
                    .ThenInclude(n => n.district)
                        .ThenInclude(d => d.city)
                .AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(p => p.price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.price <= maxPrice.Value);

            if (minBedrooms.HasValue)
                query = query.Where(p => p.bedrooms >= minBedrooms.Value);

            if (maxBedrooms.HasValue)
                query = query.Where(p => p.bedrooms <= maxBedrooms.Value);

            if (minArea.HasValue)
                query = query.Where(p => p.area >= minArea.Value);

            if (maxArea.HasValue)
                query = query.Where(p => p.area <= maxArea.Value);

            if (propertyType.HasValue)
                query = query.Where(p => p.type == (Property.PropertyType)propertyType.Value);

            if (cityId.HasValue)
                query = query.Where(p => p.neighbourhood.district.cityId == cityId.Value);

            if (districtId.HasValue)
                query = query.Where(p => p.neighbourhood.districtId == districtId.Value);

            if (neighbourhoodId.HasValue)
                query = query.Where(p => p.neighbourhoodId == neighbourhoodId.Value);

            if(!string.IsNullOrEmpty(parcelNumber))
                query = query.Where(p => p.parcelNumber == parcelNumber);

            if(!string.IsNullOrEmpty(lotNumber))
                query = query.Where(p => p.lotNumber == lotNumber);

            if (ownerId.HasValue)
                query = query.Where(p => p.ownerId == ownerId.Value);

            return await query.ToListAsync();
        }

        public async Task<int> GetPropertyCountByUser(int ownerId)
        {
            return await _dbSet.CountAsync(p => p.ownerId == ownerId);
        }
    }
}
