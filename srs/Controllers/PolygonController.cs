using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using srs.DTO;
using srs.Interfaces;
using System.Security.Claims;

namespace srs.Controllers
{
    [ApiController]
    [Authorize]
    [Tags("Polygons")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class PolygonController : ControllerBase
    {
        private readonly IPolygonService _polygonService;

        public PolygonController(IPolygonService polygonService)
        {
            _polygonService = polygonService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PolygonDTO>>> getAllPolygons([FromQuery] string? labels)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Unauthorized(new { message = "User could not be identified." });

            if (!string.IsNullOrWhiteSpace(labels))
            {
                var requestedLabels = labels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return Ok(await _polygonService.GetVisiblePolygonsByLabelsAsync(requestedLabels, currentUserId.Value, IsAdmin()));
            }

            return Ok(await _polygonService.GetVisiblePolygonsAsync(currentUserId.Value, IsAdmin()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PolygonDTO>> getPolygonById(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Unauthorized(new { message = "User could not be identified." });

            var polygon = await _polygonService.GetPolygonByIdAsync(id, currentUserId.Value, IsAdmin());
            if (polygon == null)
                return NotFound(new { message = $"Polygon with id {id} not found" });

            return Ok(polygon);
        }

        [HttpPost]
        public async Task<ActionResult<PolygonDTO>> createPolygon([FromBody] CreatePolygonDTO dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized(new { message = "User could not be identified." });

                var polygon = await _polygonService.CreatePolygonAsync(dto, currentUserId.Value);
                return CreatedAtAction(nameof(getPolygonById), new { id = polygon.id }, polygon);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<PolygonDTO>>> createPolygons([FromBody] CreatePolygonsDTO dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized(new { message = "User could not be identified." });

                var polygons = await _polygonService.CreatePolygonsAsync(dto, currentUserId.Value);
                return Ok(polygons);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("upsert")]
        public async Task<ActionResult<IEnumerable<PolygonDTO>>> upsertPolygons([FromBody] UpsertPolygonsDTO dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized(new { message = "User could not be identified." });

                var polygons = await _polygonService.UpsertPolygonsAsync(dto, currentUserId.Value);
                return Ok(polygons);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deletePolygon(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Unauthorized(new { message = "User could not be identified." });

            await _polygonService.DeletePolygonAsync(id, currentUserId.Value, IsAdmin());
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private bool IsAdmin() => User.IsInRole("Admin");
    }
}
