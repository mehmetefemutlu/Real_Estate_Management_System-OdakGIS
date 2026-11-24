using srs.Entity;

namespace srs.Interfaces
{
    public interface IPolygonRepository : IRepository<Polygon>
    {
        Task<IEnumerable<Polygon>> GetVisible(int currentUserId, bool isAdmin);
        Task<IEnumerable<Polygon>> GetVisibleByLabels(IEnumerable<string> labels, int currentUserId, bool isAdmin);
        Task<Polygon?> GetByIdForAccess(int id, int currentUserId, bool isAdmin);
        Task<Polygon?> GetByLabelForOwner(string label, int ownerId);
        Task UpsertByLabelForOwner(string label, string coordinates, int ownerId);
        Task DeleteByLabelForOwner(string label, int ownerId);
    }
}
