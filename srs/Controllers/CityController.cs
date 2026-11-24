using Microsoft.AspNetCore.Mvc;
using srs.DTO;
using srs.Interfaces;

namespace srs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;

        public CityController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityDTO>>> getAllCities()
        {
            var cities = await _cityService.GetAllCitiesAsync();
            return Ok(cities);
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<CityViewDTO>>> getAllCitiesView()
        {
            var cities = await _cityService.GetAllCitiesViewAsync();
            return Ok(cities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CityDTO>> getCityById(int id)
        {
            var city = await _cityService.GetCityByIdAsync(id);
            if (city == null)
                return NotFound(new { message = $"City with id {id} not found" });

            return Ok(city);
        }

        [HttpPost]
        public async Task<ActionResult<CityDTO>> createCity([FromBody] CreateCityDTO createCityDto)
        {
            try
            {
                var city = await _cityService.CreateCityAsync(createCityDto);
                return CreatedAtAction(nameof(getCityById), new { id = city.id }, city);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> updateCity(int id, [FromBody] UpdateCityDTO updateCityDto)
        {
            try
            {
                await _cityService.UpdateCityAsync(id, updateCityDto);
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
        public async Task<IActionResult> deleteCity(int id)
        {
            try
            {
                await _cityService.DeleteCityAsync(id);
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
        public async Task<ActionResult<bool>> cityExists(int id)
        {
            var exists = await _cityService.CityExistsAsync(id);
            return Ok(new { exists });
        }

        [HttpGet("exists/name/{cityName}")]
        public async Task<ActionResult<bool>> cityExistsByName(string cityName)
        {
            var exists = await _cityService.CityExistsByNameAsync(cityName);
            return Ok(new { exists });
        }
    }
}
