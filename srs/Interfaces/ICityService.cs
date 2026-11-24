using srs.DTO;

namespace srs.Interfaces
{
    public interface ICityService
    {
        Task<IEnumerable<CityDTO>> GetAllCitiesAsync();
        Task<IEnumerable<CityViewDTO>> GetAllCitiesViewAsync();
        Task<CityDTO?> GetCityByIdAsync(int id);
        Task<CityDTO> CreateCityAsync(CreateCityDTO createCityDto);
        Task UpdateCityAsync(int id, UpdateCityDTO updateCityDto);
        Task DeleteCityAsync(int id);
        Task<bool> CityExistsAsync(int id);
        Task<bool> CityExistsByNameAsync(string cityName);
    }
}
