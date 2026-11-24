using srs.DTO;
using srs.Entity;
using srs.Interfaces;

namespace srs.Services
{
    public class PolygonService : IPolygonService
    {
        private readonly IPolygonRepository _polygonRepository;

        public PolygonService(IPolygonRepository polygonRepository)
        {
            _polygonRepository = polygonRepository;
        }

        public async Task<IEnumerable<PolygonDTO>> GetVisiblePolygonsAsync(int currentUserId, bool isAdmin)
        {
            var polygons = await _polygonRepository.GetVisible(currentUserId, isAdmin);
            return polygons.Select(MapToDto);
        }

        public async Task<IEnumerable<PolygonDTO>> GetVisiblePolygonsByLabelsAsync(IEnumerable<string> labels, int currentUserId, bool isAdmin)
        {
            var polygons = await _polygonRepository.GetVisibleByLabels(labels, currentUserId, isAdmin);
            return polygons.Select(MapToDto);
        }

        public async Task<PolygonDTO?> GetPolygonByIdAsync(int id, int currentUserId, bool isAdmin)
        {
            var polygon = await _polygonRepository.GetByIdForAccess(id, currentUserId, isAdmin);
            return polygon == null ? null : MapToDto(polygon);
        }

        public async Task<PolygonDTO> CreatePolygonAsync(CreatePolygonDTO dto, int ownerId)
        {
            Validate(dto);
            var polygon = await _polygonRepository.Add(new Polygon
            {
                ownerId = ownerId,
                label = dto.label.Trim(),
                coordinates = dto.coordinates.Trim(),
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
            });

            return MapToDto(polygon);
        }

        public async Task<IEnumerable<PolygonDTO>> CreatePolygonsAsync(CreatePolygonsDTO dto, int ownerId)
        {
            var created = new List<PolygonDTO>();
            foreach (var polygon in dto.polygons)
            {
                created.Add(await CreatePolygonAsync(polygon, ownerId));
            }

            return created;
        }

        public async Task<IEnumerable<PolygonDTO>> UpsertPolygonsAsync(UpsertPolygonsDTO dto, int ownerId)
        {
            foreach (var polygon in dto.polygons)
            {
                Validate(polygon);
                await _polygonRepository.UpsertByLabelForOwner(polygon.label.Trim(), polygon.coordinates.Trim(), ownerId);
            }

            return await GetVisiblePolygonsByLabelsAsync(dto.polygons.Select(p => p.label), ownerId, false);
        }

        public async Task UpsertPolygonByLabelAsync(string label, string coordinates, int ownerId)
        {
            Validate(new CreatePolygonDTO { label = label, coordinates = coordinates });
            await _polygonRepository.UpsertByLabelForOwner(label.Trim(), coordinates.Trim(), ownerId);
        }

        public async Task DeletePolygonByLabelAsync(string label, int ownerId)
        {
            if (string.IsNullOrWhiteSpace(label)) return;
            await _polygonRepository.DeleteByLabelForOwner(label.Trim(), ownerId);
        }

        public async Task DeletePolygonAsync(int id, int currentUserId, bool isAdmin)
        {
            var polygon = await _polygonRepository.GetByIdForAccess(id, currentUserId, isAdmin);
            if (polygon == null) return;

            await _polygonRepository.Delete(id);
        }

        private static PolygonDTO MapToDto(Polygon polygon)
        {
            return new PolygonDTO
            {
                id = polygon.id,
                ownerId = polygon.ownerId,
                label = polygon.label,
                coordinates = polygon.coordinates,
                createdAt = polygon.createdAt,
                updatedAt = polygon.updatedAt
            };
        }

        private static void Validate(CreatePolygonDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.label) || string.IsNullOrWhiteSpace(dto.coordinates))
                throw new InvalidOperationException("Polygon label and coordinates are required.");
        }
    }
}
