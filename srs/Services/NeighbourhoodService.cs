using srs.DTO;
using srs.Entity;
using srs.Interfaces;

namespace srs.Services
{
    public class NeighbourhoodService : INeighbourhoodService
    {
        private readonly INeighbourhoodRepository _neighbourhoodRepository;
        private readonly IDistrictRepository _districtRepository;

        public NeighbourhoodService(INeighbourhoodRepository neighbourhoodRepository, IDistrictRepository districtRepository)
        {
            _neighbourhoodRepository = neighbourhoodRepository;
            _districtRepository = districtRepository;
        }

        public async Task<IEnumerable<NeighbourhoodDTO>> GetAllNeighbourhoodsAsync()
        {
            var neighbourhoods = await _neighbourhoodRepository.GetAllWithPropertiesAndDistrict();
            return neighbourhoods.Select(n => new NeighbourhoodDTO
            {
                id = n.id,
                name = n.name,
                districtId = n.districtId,
                districtName = n.district?.districtName ?? string.Empty,
                postalCode = n.district?.postalCode ?? string.Empty,
                cityName = n.district?.city?.cityName ?? string.Empty,
                properties = n.properties?.Select(p => new PropertyViewDTO
                {
                    propertyID = p.id,
                    title = p.title,
                    price = p.price,
                    address = p.address,
                    parcelNumber = p.parcelNumber,
                    lotNumber = p.lotNumber,
                    area = p.area,
                    propertyType = p.type,
                    districtName = n.district?.districtName ?? string.Empty,
                    cityName = n.district?.city?.cityName ?? string.Empty,
                    createdAt = p.createdAt
                }).ToList() ?? new List<PropertyViewDTO>()
            }).ToList();
        }

        public async Task<IEnumerable<NeighbourhoodViewDTO>> GetAllNeighbourhoodsViewAsync()
        {
            var neighbourhoods = await _neighbourhoodRepository.GetAllWithDistrict();
            return neighbourhoods.Select(n => new NeighbourhoodViewDTO
            {
                id = n.id,
                name = n.name,
                districtName = n.district?.districtName ?? string.Empty,
                cityName = n.district?.city?.cityName ?? string.Empty
            });
        }

        public async Task<NeighbourhoodDTO?> GetNeighbourhoodByIdAsync(int id)
        {
            var neighbourhood = await _neighbourhoodRepository.GetNeighbourhoodWithProperties(id);
            if (neighbourhood == null) return null;

            return new NeighbourhoodDTO
            {
                id = neighbourhood.id,
                name = neighbourhood.name,
                districtId = neighbourhood.districtId,
                districtName = neighbourhood.district?.districtName ?? string.Empty,
                postalCode = neighbourhood.district?.postalCode ?? string.Empty,
                cityName = neighbourhood.district?.city?.cityName ?? string.Empty,
                properties = neighbourhood.properties?.Select(p => new PropertyViewDTO
                {
                    propertyID = p.id,
                    title = p.title,
                    price = p.price,
                    address = p.address,
                    parcelNumber = p.parcelNumber,
                    lotNumber = p.lotNumber,
                    area = p.area,
                    propertyType = p.type,
                    districtName = neighbourhood.district?.districtName ?? string.Empty,
                    cityName = neighbourhood.district?.city?.cityName ?? string.Empty,
                    createdAt = p.createdAt
                }).ToList() ?? new List<PropertyViewDTO>()
            };
        }

        public async Task<IEnumerable<NeighbourhoodViewDTO>> GetNeighbourhoodsByDistrictAsync(int districtId)
        {
            var neighbourhoods = await _neighbourhoodRepository.GetNeighbourhoodsByDistrict(districtId);
            return neighbourhoods.Select(n => new NeighbourhoodViewDTO
            {
                id = n.id,
                name = n.name,
                districtName = n.district?.districtName ?? string.Empty,
                cityName = n.district?.city?.cityName ?? string.Empty
            });
        }

        public async Task<NeighbourhoodDTO> CreateNeighbourhoodAsync(CreateNeighbourhoodDTO createNeighbourhoodDto)
        {
            var districtExists = await _districtRepository.DistrictExists(createNeighbourhoodDto.districtId);
            if (!districtExists)
                throw new KeyNotFoundException($"District with id {createNeighbourhoodDto.districtId} not found");

            var neighbourhood = new Neighbourhood
            {
                name = createNeighbourhoodDto.name,
                districtId = createNeighbourhoodDto.districtId,
                properties = new List<Property>()
            };

            var createdNeighbourhood = await _neighbourhoodRepository.Add(neighbourhood);
            var neighbourhoodWithDistrict = await _neighbourhoodRepository.GetNeighbourhoodWithProperties(createdNeighbourhood.id);

            return new NeighbourhoodDTO
            {
                id = neighbourhoodWithDistrict!.id,
                name = neighbourhoodWithDistrict.name,
                districtId = neighbourhoodWithDistrict.districtId,
                districtName = neighbourhoodWithDistrict.district?.districtName ?? string.Empty,
                postalCode = neighbourhoodWithDistrict.district?.postalCode ?? string.Empty,
                cityName = neighbourhoodWithDistrict.district?.city?.cityName ?? string.Empty,
                properties = new List<PropertyViewDTO>()
            };
        }

        public async Task UpdateNeighbourhoodAsync(int id, UpdateNeighbourhoodDTO updateNeighbourhoodDto)
        {
            var neighbourhood = await _neighbourhoodRepository.GetById(id);
            if (neighbourhood == null)
                throw new KeyNotFoundException($"Neighbourhood with id {id} not found");

            if (!string.IsNullOrEmpty(updateNeighbourhoodDto.name))
                neighbourhood.name = updateNeighbourhoodDto.name;

            if (updateNeighbourhoodDto.districtId.HasValue)
            {
                var districtExists = await _districtRepository.DistrictExists(updateNeighbourhoodDto.districtId.Value);
                if (!districtExists)
                    throw new KeyNotFoundException($"District with id {updateNeighbourhoodDto.districtId.Value} not found");
                neighbourhood.districtId = updateNeighbourhoodDto.districtId.Value;
            }

            await _neighbourhoodRepository.Update(neighbourhood);
        }

        public async Task DeleteNeighbourhoodAsync(int id)
        {
            var exists = await _neighbourhoodRepository.Exists(id);
            if (!exists)
                throw new KeyNotFoundException($"Neighbourhood with id {id} not found");

            await _neighbourhoodRepository.Delete(id);
        }

        public async Task<bool> NeighbourhoodExistsAsync(int id)
        {
            return await _neighbourhoodRepository.NeighbourhoodExists(id);
        }
    }
}
