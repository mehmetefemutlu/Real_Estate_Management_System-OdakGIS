using srs.DTO;

namespace srs.Interfaces
{
    public interface IPolygonService
    {
        Task<IEnumerable<PolygonDTO>> GetVisiblePolygonsAsync(int currentUserId, bool isAdmin);
        Task<IEnumerable<PolygonDTO>> GetVisiblePolygonsByLabelsAsync(IEnumerable<string> labels, int currentUserId, bool isAdmin);
        Task<PolygonDTO?> GetPolygonByIdAsync(int id, int currentUserId, bool isAdmin);
        Task<PolygonDTO> CreatePolygonAsync(CreatePolygonDTO dto, int ownerId);
        Task<IEnumerable<PolygonDTO>> CreatePolygonsAsync(CreatePolygonsDTO dto, int ownerId);
        Task<IEnumerable<PolygonDTO>> UpsertPolygonsAsync(UpsertPolygonsDTO dto, int ownerId);
        Task UpsertPolygonByLabelAsync(string label, string coordinates, int ownerId);
        Task DeletePolygonByLabelAsync(string label, int ownerId);
        Task DeletePolygonAsync(int id, int currentUserId, bool isAdmin);
    }
}
