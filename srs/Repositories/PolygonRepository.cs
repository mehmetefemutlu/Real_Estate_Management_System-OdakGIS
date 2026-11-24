using Microsoft.EntityFrameworkCore;
using srs.Data;
using srs.Entity;
using srs.Interfaces;

namespace srs.Repositories
{
    public class PolygonRepository : Repository<Polygon>, IPolygonRepository
    {
        public PolygonRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Polygon>> GetVisible(int currentUserId, bool isAdmin)
        {
            var query = _dbSet.AsQueryable();
            if (!isAdmin)
                query = query.Where(p => p.ownerId == currentUserId);

            return await query
                .OrderByDescending(p => p.updatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Polygon>> GetVisibleByLabels(IEnumerable<string> labels, int currentUserId, bool isAdmin)
        {
            var normalized = labels
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Distinct()
                .ToList();

            var query = _dbSet.Where(p => normalized.Contains(p.label));
            if (!isAdmin)
                query = query.Where(p => p.ownerId == currentUserId);

            return await query
                .OrderByDescending(p => p.updatedAt)
                .ToListAsync();
        }

        public async Task<Polygon?> GetByIdForAccess(int id, int currentUserId, bool isAdmin)
        {
            var query = _dbSet.Where(p => p.id == id);
            if (!isAdmin)
                query = query.Where(p => p.ownerId == currentUserId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Polygon?> GetByLabelForOwner(string label, int ownerId)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.label == label && p.ownerId == ownerId);
        }

        public async Task UpsertByLabelForOwner(string label, string coordinates, int ownerId)
        {
            var polygon = await GetByLabelForOwner(label, ownerId);
            if (polygon == null)
            {
                await Add(new Polygon
                {
                    ownerId = ownerId,
                    label = label,
                    coordinates = coordinates,
                    createdAt = DateTime.UtcNow,
                    updatedAt = DateTime.UtcNow
                });
                return;
            }

            polygon.coordinates = coordinates;
            polygon.updatedAt = DateTime.UtcNow;
            await Update(polygon);
        }

        public async Task DeleteByLabelForOwner(string label, int ownerId)
        {
            var polygon = await GetByLabelForOwner(label, ownerId);
            if (polygon == null) return;

            _dbSet.Remove(polygon);
            await _context.SaveChangesAsync();
        }
    }
}
