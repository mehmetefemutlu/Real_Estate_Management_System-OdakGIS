using srs.DTO;
using srs.Entity;
using srs.Interfaces;

namespace srs.Services
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _cityRepository;

        public CityService(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task<IEnumerable<CityDTO>> GetAllCitiesAsync()
        {
            var cities = await _cityRepository.GetCitiesWithDistricts();
            return cities.Select(c => new CityDTO
            {
                id = c.id,
                cityName = c.cityName,
                districts = c.districts?.Select(d => new DistrictDTO
                {
                    id = d.id,
                    districtName = d.districtName,
                    cityId = d.city != null ? d.city.id : 0,
                    cityName = d.city != null ? d.city.cityName : string.Empty,
                    postalCode = d.postalCode,
                    neighbourhoods = d.neighbourhoods?.Select(n => new NeighbourhoodViewDTO
                    {
                        id = n.id,
                        name = n.name,
                        districtName = d.districtName,
                        cityName = c.cityName
                    }).ToList()
                }).ToList()
            });
        }

        public async Task<IEnumerable<CityViewDTO>> GetAllCitiesViewAsync()
        {
            var cities = await _cityRepository.GetAll();
            return cities.Select(c => new CityViewDTO
            {
                id = c.id,
                cityName = c.cityName
            });
        }

        public async Task<CityDTO?> GetCityByIdAsync(int id)
        {
            var city = await _cityRepository.GetCityWithDistricts(id);
            if (city == null) return null;

            return new CityDTO
            {
                id = city.id,
                cityName = city.cityName,
                districts = city.districts?.Select(d => new DistrictDTO
                {
                    id = d.id,
                    districtName = d.districtName,
                    cityId = d.city != null ? d.city.id : 0 ,
                    postalCode = d.postalCode,
                    neighbourhoods = d.neighbourhoods?.Select(n => new NeighbourhoodViewDTO
                    {
                        id = n.id,
                        name = n.name,
                        districtName = d.districtName,
                        cityName = city.cityName
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<CityDTO> CreateCityAsync(CreateCityDTO createCityDto)
        {
            var city = new City
            {
                cityName = createCityDto.cityName,
                districts = new List<District>()
            };

            var createdCity = await _cityRepository.Add(city);

            return new CityDTO
            {
                id = createdCity.id,
                cityName = createdCity.cityName,
                districts = new List<DistrictDTO>()
            };
        }

        public async Task UpdateCityAsync(int id, UpdateCityDTO updateCityDto)
        {
            var city = await _cityRepository.GetById(id);
            if (city == null)
                throw new KeyNotFoundException($"City with id {id} not found");

            city.cityName = updateCityDto.cityName;
            await _cityRepository.Update(city);
        }

        public async Task DeleteCityAsync(int id)
        {
            var exists = await _cityRepository.Exists(id);
            if (!exists)
                throw new KeyNotFoundException($"City with id {id} not found");

            await _cityRepository.Delete(id);
        }

        public async Task<bool> CityExistsAsync(int id)
        {
            return await _cityRepository.Exists(id);
        }

        public async Task<bool> CityExistsByNameAsync(string cityName)
        {
            return await _cityRepository.CityExistsByName(cityName);
        }
    }
}
