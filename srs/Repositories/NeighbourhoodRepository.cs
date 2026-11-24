using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using srs.Data;
using srs.Entity;
using srs.Interfaces;

namespace srs.Repositories
{
    public class NeighbourhoodRepository : Repository<Neighbourhood>, INeighbourhoodRepository
    {
        public NeighbourhoodRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Neighbourhood>> GetAllWithDistrict()
        {
            return await _dbSet
                .Include(n => n.district)
                    .ThenInclude(d => d.city)
                .ToListAsync();
        }

        public async Task<IEnumerable<Neighbourhood>> GetAllWithPropertiesAndDistrict()
        {
            return await _dbSet
                .Include(n => n.properties)
                .Include(n => n.district)
                    .ThenInclude(d => d.city)
                .ToListAsync();
        }

        public async Task<Neighbourhood?> GetNeighbourhoodWithProperties(int id)
        {
            return await _dbSet
                .Include(n => n.properties)
                .Include(n => n.district)
                    .ThenInclude(d => d.city)
                .FirstOrDefaultAsync(n => n.id == id);
        }

        public async Task<Neighbourhood?> GetByLocationNames(string cityName, string districtName, string neighbourhoodName)
        {
            var neighbourhoods = await _dbSet
                .Include(n => n.district)
                    .ThenInclude(d => d.city)
                .Where(n => n.district != null && n.district.city != null)
                .ToListAsync();

            return neighbourhoods.FirstOrDefault(n =>
                NamesMatch(n.district!.city!.cityName, cityName) &&
                NamesMatch(n.district.districtName, districtName) &&
                NamesMatch(n.name, neighbourhoodName));
        }

        public async Task<IEnumerable<Neighbourhood>> GetNeighbourhoodsByDistrict(int districtId)
        {
            return await _dbSet
                .Include(n => n.district)
                    .ThenInclude(d => d.city)
                .Where(n => n.districtId == districtId)
                .ToListAsync();
        }

        public async Task<bool> NeighbourhoodExists(int neighbourhoodId)
        {
            return await _dbSet.AnyAsync(n => n.id == neighbourhoodId);
        }

        private static bool NamesMatch(string storedName, string inputName)
        {
            return NormalizeLocationName(storedName) == NormalizeLocationName(inputName);
        }

        private static string NormalizeLocationName(string value)
        {
            var cleaned = value
                .Trim()
                .ToLowerInvariant()
                .Replace('ı', 'i')
                .Replace('İ', 'i');

            var normalized = cleaned.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;

                builder.Append(char.IsLetterOrDigit(character) ? character : ' ');
            }

            var words = builder
                .ToString()
                .Normalize(NormalizationForm.FormC)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (words.Count > 0 && (words[^1] == "mh" || words[^1] == "mah" || words[^1] == "mahallesi"))
                words.RemoveAt(words.Count - 1);

            return string.Join(' ', words);
        }
    }
}
