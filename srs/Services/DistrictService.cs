using srs.DTO;
using srs.Entity;
using srs.Interfaces;

namespace srs.Services
{
    public class DistrictService : IDistrictService
    {
        private readonly IDistrictRepository _districtRepository;
        private readonly ICityRepository _cityRepository;

        public DistrictService(IDistrictRepository districtRepository, ICityRepository cityRepository)
        {
            _districtRepository = districtRepository;
            _cityRepository = cityRepository;
        }

        public async Task<IEnumerable<DistrictDTO>> GetAllDistrictsAsync()
        {
            var districts = await _districtRepository.GetAllWithCity();
            return districts.Select(d => new DistrictDTO
            {
                id = d.id,
                districtName = d.districtName,
                cityId = d.cityId,
                cityName = d.city?.cityName ?? string.Empty,
                postalCode = d.postalCode,
                neighbourhoods = GetDistrictByIdAsync(d.id).Result?.neighbourhoods
            });
        }

        public async Task<IEnumerable<DistrictViewDTO>> GetAllDistrictsViewAsync()
        {
            var districts = await _districtRepository.GetAllWithCity();
            return districts.Select(d => new DistrictViewDTO
            {
                id = d.id,
                districtName = d.districtName,
                postalCode = d.postalCode,
                cityName = d.city?.cityName ?? string.Empty
            });
        }

        public async Task<DistrictDTO?> GetDistrictByIdAsync(int id)
        {
            var district = await _districtRepository.GetDistrictWithNeighbourhoods(id);
            if (district == null) return null;

            return new DistrictDTO
            {
                id = district.id,
                districtName = district.districtName,
                cityId = district.cityId,
                cityName = district.city?.cityName ?? string.Empty,
                postalCode = district.postalCode,
                neighbourhoods = district.neighbourhoods?.Select(n => new NeighbourhoodViewDTO
                {
                    id = n.id,
                    name = n.name,
                    districtName = district.districtName,
                    cityName = district.city?.cityName ?? string.Empty
                }).ToList()
            };
        }

        public async Task<IEnumerable<DistrictViewDTO>> GetDistrictsByCityAsync(int cityId)
        {
            var cityExists = await _cityRepository.Exists(cityId);
            if (!cityExists)
                throw new KeyNotFoundException($"City with id {cityId} not found");


            var districts = await _districtRepository.GetDistrictsByCity(cityId);
            return districts.Select(d => new DistrictViewDTO
            {
                id = d.id,
                districtName = d.districtName,
                postalCode = d.postalCode,
                cityName = d.city?.cityName ?? string.Empty
            });
        }

        public async Task<DistrictDTO> CreateDistrictAsync(CreateDistrictDTO createDistrictDto)
        {
            var cityExists = await _cityRepository.Exists(createDistrictDto.cityId);
            if (!cityExists)
                throw new KeyNotFoundException($"City with id {createDistrictDto.cityId} not found");

            var district = new District
            {
                districtName = createDistrictDto.districtName,
                cityId = createDistrictDto.cityId,
                postalCode = createDistrictDto.postalCode,
                neighbourhoods = new List<Neighbourhood>()
            };

            var createdDistrict = await _districtRepository.Add(district);

            var districtWithCity = await _districtRepository.GetDistrictWithNeighbourhoods(createdDistrict.id);

            return new DistrictDTO
            {
                id = districtWithCity!.id,
                districtName = districtWithCity.districtName,
                cityId = districtWithCity.cityId,
                cityName = districtWithCity.city?.cityName ?? string.Empty,
                postalCode = districtWithCity.postalCode,
                neighbourhoods = new List<NeighbourhoodViewDTO>()
            };
        }

        public async Task UpdateDistrictAsync(int id, UpdateDistrictDTO updateDistrictDto)
        {
            var district = await _districtRepository.GetById(id);
            if (district == null)
                throw new KeyNotFoundException($"District with id {id} not found");

            if (!string.IsNullOrEmpty(updateDistrictDto.districtName))
                district.districtName = updateDistrictDto.districtName;

            if (!string.IsNullOrEmpty(updateDistrictDto.postalCode))
                district.postalCode = updateDistrictDto.postalCode;

            if (updateDistrictDto.cityId.HasValue)
            {
                var cityExists = await _cityRepository.Exists(updateDistrictDto.cityId.Value);
                if (!cityExists)
                    throw new KeyNotFoundException($"City with id {updateDistrictDto.cityId.Value} not found");
                district.cityId = updateDistrictDto.cityId.Value;
            }

            await _districtRepository.Update(district);
        }

        public async Task DeleteDistrictAsync(int id)
        {
            var exists = await _districtRepository.Exists(id);
            if (!exists)
                throw new KeyNotFoundException($"District with id {id} not found");

            await _districtRepository.Delete(id);
        }

        public async Task<bool> DistrictExistsAsync(int id)
        {
            return await _districtRepository.DistrictExists(id);
        }
    }
}
