using Microsoft.AspNetCore.Mvc;
using srs.DTO;
using srs.Interfaces;

namespace srs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NeighbourhoodController : ControllerBase
    {
        private readonly INeighbourhoodService _neighbourhoodService;

        public NeighbourhoodController(INeighbourhoodService neighbourhoodService)
        {
            _neighbourhoodService = neighbourhoodService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NeighbourhoodDTO>>> getAllNeighbourhoods()
        {
            var neighbourhoods = await _neighbourhoodService.GetAllNeighbourhoodsAsync();
            return Ok(neighbourhoods);
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<NeighbourhoodViewDTO>>> getAllNeighbourhoodsView()
        {
            var neighbourhoods = await _neighbourhoodService.GetAllNeighbourhoodsViewAsync();
            return Ok(neighbourhoods);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NeighbourhoodDTO>> getNeighbourhoodById(int id)
        {
            var neighbourhood = await _neighbourhoodService.GetNeighbourhoodByIdAsync(id);
            if (neighbourhood == null)
                return NotFound(new { message = $"Neighbourhood with id {id} not found" });

            return Ok(neighbourhood);
        }

        [HttpGet("district/{districtId}")]
        public async Task<ActionResult<IEnumerable<NeighbourhoodViewDTO>>> getNeighbourhoodsByDistrict(int districtId)
        {
            var neighbourhoods = await _neighbourhoodService.GetNeighbourhoodsByDistrictAsync(districtId);
            return Ok(neighbourhoods);
        }

        [HttpPost]
        public async Task<ActionResult<NeighbourhoodDTO>> createNeighbourhood([FromBody] CreateNeighbourhoodDTO createNeighbourhoodDto)
        {
            try
            {
                var neighbourhood = await _neighbourhoodService.CreateNeighbourhoodAsync(createNeighbourhoodDto);
                return CreatedAtAction(nameof(getNeighbourhoodById), new { id = neighbourhood.id }, neighbourhood);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> updateNeighbourhood(int id, [FromBody] UpdateNeighbourhoodDTO updateNeighbourhoodDto)
        {
            try
            {
                await _neighbourhoodService.UpdateNeighbourhoodAsync(id, updateNeighbourhoodDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteNeighbourhood(int id)
        {
            try
            {
                await _neighbourhoodService.DeleteNeighbourhoodAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> neighbourhoodExists(int id)
        {
            var exists = await _neighbourhoodService.NeighbourhoodExistsAsync(id);
            return Ok(new { exists });
        }
    }
}
