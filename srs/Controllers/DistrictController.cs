using Microsoft.AspNetCore.Mvc;
using srs.DTO;
using srs.Interfaces;

namespace srs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistrictController : ControllerBase
    {
        private readonly IDistrictService _districtService;

        public DistrictController(IDistrictService districtService)
        {
            _districtService = districtService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DistrictDTO>>> getAllDistricts()
        {
            var districts = await _districtService.GetAllDistrictsAsync();
            return Ok(districts);
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<DistrictViewDTO>>> getAllDistrictsView()
        {
            var districts = await _districtService.GetAllDistrictsViewAsync();
            return Ok(districts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DistrictDTO>> getDistrictById(int id)
        {
            var district = await _districtService.GetDistrictByIdAsync(id);
            if (district == null)
                return NotFound(new { message = $"District with id {id} not found" });

            return Ok(district);
        }

        [HttpGet("city/{cityId}")]
        public async Task<ActionResult<IEnumerable<DistrictViewDTO>>> getDistrictsByCity(int cityId)
        {
            var districts = await _districtService.GetDistrictsByCityAsync(cityId);
            return Ok(districts);
        }

        [HttpPost]
        public async Task<ActionResult<DistrictDTO>> createDistrict([FromBody] CreateDistrictDTO createDistrictDto)
        {
            try
            {
                var district = await _districtService.CreateDistrictAsync(createDistrictDto);
                return CreatedAtAction(nameof(getDistrictById), new { id = district.id }, district);
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
        public async Task<IActionResult> updateDistrict(int id, [FromBody] UpdateDistrictDTO updateDistrictDto)
        {
            try
            {
                await _districtService.UpdateDistrictAsync(id, updateDistrictDto);
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
        public async Task<IActionResult> deleteDistrict(int id)
        {
            try
            {
                await _districtService.DeleteDistrictAsync(id);
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
        public async Task<ActionResult<bool>> districtExists(int id)
        {
            var exists = await _districtService.DistrictExistsAsync(id);
            return Ok(new { exists });
        }
    }
}
