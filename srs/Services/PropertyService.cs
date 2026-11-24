using srs.DTO;
using srs.Entity;
using srs.Interfaces;

namespace srs.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly IPropertyRepository _propertyRepository;
        private readonly INeighbourhoodRepository _neighbourhoodRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogRepository _logRepository;
        private readonly IPolygonService _polygonService;

        public PropertyService(
            IPropertyRepository propertyRepository,
            INeighbourhoodRepository neighbourhoodRepository,
            IUserRepository userRepository,
            ILogRepository logRepository,
            IPolygonService polygonService)
        {
            _propertyRepository = propertyRepository;
            _neighbourhoodRepository = neighbourhoodRepository;
            _userRepository = userRepository;
            _logRepository = logRepository;
            _polygonService = polygonService;
        }

        public async Task<IEnumerable<PropertyDTO>> GetAllPropertiesAsync()
        {
            var properties = await _propertyRepository.GetAll();
            return properties.Select(MapToPropertyDTO).ToList();
        }

        public async Task<IEnumerable<PropertyViewDTO>> GetAllPropertiesViewAsync()
        {
            var properties = await _propertyRepository.GetAll();
            return properties.Select(p => new PropertyViewDTO
            {
                propertyID = p.id,
                title = p.title,
                price = p.price,
                address = p.address,
                parcelNumber = p.parcelNumber,
                lotNumber = p.lotNumber,
                area = p.area,
                coordinates = p.coordinates,
                propertyType = p.type,
                isReadOnly = p.isReadOnly,
                districtName = p.neighbourhood?.district?.districtName ?? string.Empty,
                cityName = p.neighbourhood?.district?.city?.cityName ?? string.Empty,
                createdAt = p.createdAt
            });
        }

        public async Task<PropertyDTO?> GetPropertyByIdAsync(int id)
        {
            var property = await _propertyRepository.GetPropertyWithDetails(id);
            if (property == null) return null;

            return MapToPropertyDTO(property);
        }

        public async Task<IEnumerable<PropertyViewDTO>> GetPropertiesByUserAsync(int userId)
        {
            var properties = await _propertyRepository.GetPropertiesByUser(userId);
            return properties.Select(p => new PropertyViewDTO
            {
                propertyID = p.id,
                title = p.title,
                price = p.price,
                address = p.address,
                parcelNumber= p.parcelNumber,
                lotNumber = p.lotNumber,
                area = p.area,
                coordinates = p.coordinates,
                propertyType = p.type,
                isReadOnly = p.isReadOnly,
                districtName = p.neighbourhood?.district?.districtName ?? string.Empty,
                cityName = p.neighbourhood?.district?.city?.cityName ?? string.Empty,
                createdAt = p.createdAt
            });
        }

        public async Task<IEnumerable<PropertyViewDTO>> GetPropertiesByNeighbourhoodAsync(int neighbourhoodId)
        {
            var properties = await _propertyRepository.GetPropertiesByNeighbourhood(neighbourhoodId);
            return properties.Select(p => new PropertyViewDTO
            {
                propertyID = p.id,
                title = p.title,
                price = p.price,
                address = p.address,
                parcelNumber = p.parcelNumber,
                lotNumber = p.lotNumber,
                area = p.area,
                coordinates = p.coordinates,
                propertyType = p.type,
                isReadOnly = p.isReadOnly,
                districtName = p.neighbourhood?.district?.districtName ?? string.Empty,
                cityName = p.neighbourhood?.district?.city?.cityName ?? string.Empty,
                createdAt = p.createdAt
            });
        }

        public async Task<IEnumerable<PropertyViewDTO>> GetPropertiesByDistrictAsync(int districtId)
        {
            var properties = await _propertyRepository.GetPropertiesByDistrict(districtId);
            return properties.Select(p => new PropertyViewDTO
            {
                propertyID = p.id,
                title = p.title,
                price = p.price,
                address = p.address,
                parcelNumber= p.parcelNumber,
                lotNumber= p.lotNumber,
                area = p.area,
                coordinates = p.coordinates,
                propertyType = p.type,
                isReadOnly = p.isReadOnly,
                districtName = p.neighbourhood?.district?.districtName ?? string.Empty,
                cityName = p.neighbourhood?.district?.city?.cityName ?? string.Empty,
                createdAt = p.createdAt
            });
        }

        public async Task<IEnumerable<PropertyViewDTO>> GetPropertiesByCityAsync(int cityId)
        {
            var properties = await _propertyRepository.GetPropertiesByCity(cityId);
            return properties.Select(p => new PropertyViewDTO
            {
                propertyID = p.id,
                title = p.title,
                price = p.price,
                address = p.address,
                parcelNumber = p.parcelNumber,
                lotNumber = p.lotNumber,
                area = p.area,
                coordinates = p.coordinates,
                propertyType = p.type,
                districtName = p.neighbourhood?.district?.districtName ?? string.Empty,
                cityName = p.neighbourhood?.district?.city?.cityName ?? string.Empty,
                createdAt = p.createdAt
            });
        }

        public async Task<IEnumerable<PropertyDTO>> SearchPropertiesAsync(SearchPropertyDTO searchDto)
        {
            var properties = await _propertyRepository.SearchProperties(
                searchDto.minPrice,
                searchDto.maxPrice,
                searchDto.minBedrooms,
                searchDto.maxBedrooms,
                searchDto.minArea,
                searchDto.maxArea,
                searchDto.propertyType,
                searchDto.cityId,
                searchDto.districtId,
                searchDto.neighbourhoodId,
                searchDto.parcelNumber,
                searchDto.lotNumber,
                searchDto.ownerId
            );

            return properties.Select(MapToPropertyDTO).ToList();
        }

        public async Task<PropertyDTO> CreatePropertyAsync(CreatePropertyDTO createPropertyDto, string userIP = "")
        {
            ValidateCreateProperty(createPropertyDto);

            var neighbourhoodExists = await _neighbourhoodRepository.NeighbourhoodExists(createPropertyDto.neighbourhoodId);
            if (!neighbourhoodExists)
                throw new KeyNotFoundException($"Neighbourhood with id {createPropertyDto.neighbourhoodId} not found");

            var userExists = await _userRepository.Exists(createPropertyDto.ownerId);
            if (!userExists)
                throw new KeyNotFoundException($"User with id {createPropertyDto.ownerId} not found");

            var property = new Property
            {
                title = createPropertyDto.title,
                description = createPropertyDto.description,
                price = createPropertyDto.price,
                address = createPropertyDto.address,
                ownerId = createPropertyDto.ownerId,
                parcelNumber = createPropertyDto.parcelNumber,
                lotNumber = createPropertyDto.lotNumber,
                area = createPropertyDto.area,
                bedrooms = createPropertyDto.bedrooms,
                bathrooms = createPropertyDto.bathrooms,
                parkingSpaces = createPropertyDto.parkingSpaces,
                type = createPropertyDto.propertyType,
                neighbourhoodId = createPropertyDto.neighbourhoodId,
                coordinates = createPropertyDto.coordinates ?? string.Empty,
                imagePath = createPropertyDto.imagePath ?? string.Empty,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                isReadOnly = createPropertyDto.isReadOnly
            };

            var createdProperty = await _propertyRepository.Add(property);
            await SyncPropertyPolygonAsync(createdProperty.id, createdProperty.ownerId, createdProperty.coordinates);
            await WriteLogAsync(
                createdProperty.ownerId,
                Log.OperationType.Create,
                "Property created",
                $"Property #{createdProperty.id} '{createdProperty.title}' was created.",
                userIP);
            var propertyWithDetails = await _propertyRepository.GetPropertyWithDetails(createdProperty.id);
            return MapToPropertyDTO(propertyWithDetails!);
        }

        public async Task UpdatePropertyAsync(int id, UpdatePropertyDTO updatePropertyDto, string userIP = "")
        {
            ValidateUpdateProperty(updatePropertyDto);

            var property = await _propertyRepository.GetById(id);
            if (property == null)
                throw new KeyNotFoundException($"Property with id {id} not found");

            if (!string.IsNullOrEmpty(updatePropertyDto.title))
                property.title = updatePropertyDto.title;

            if (!string.IsNullOrEmpty(updatePropertyDto.description))
                property.description = updatePropertyDto.description;

            if (updatePropertyDto.price.HasValue)
                property.price = updatePropertyDto.price.Value;

            if (!string.IsNullOrEmpty(updatePropertyDto.address))
                property.address = updatePropertyDto.address;

            if (updatePropertyDto.isReadOnly.HasValue)
                property.isReadOnly = updatePropertyDto.isReadOnly.Value;

            if (!string.IsNullOrEmpty(updatePropertyDto.parcelNumber))
                property.parcelNumber = updatePropertyDto.parcelNumber;

            if (!string.IsNullOrEmpty(updatePropertyDto.lotNumber))
                property.lotNumber = updatePropertyDto.lotNumber;

            if (updatePropertyDto.area.HasValue)
                property.area = updatePropertyDto.area.Value;

            if (!string.IsNullOrEmpty(updatePropertyDto.coordinates))
                property.coordinates = updatePropertyDto.coordinates;

            if (!string.IsNullOrEmpty(updatePropertyDto.imagePath))
                property.imagePath = updatePropertyDto.imagePath;

            if (updatePropertyDto.bedrooms.HasValue)
                property.bedrooms = updatePropertyDto.bedrooms.Value;

            if (updatePropertyDto.bathrooms.HasValue)
                property.bathrooms = updatePropertyDto.bathrooms.Value;

            if (updatePropertyDto.parkingSpaces.HasValue)
                property.parkingSpaces = updatePropertyDto.parkingSpaces.Value;

            if (Convert.ToInt32(updatePropertyDto.propertyType) >= 0 && Convert.ToInt32(updatePropertyDto.propertyType) < Enum.GetValues(typeof(Property.PropertyType)).Length)
                property.type = (Property.PropertyType)Convert.ToInt32(updatePropertyDto.propertyType);

            if (updatePropertyDto.neighbourhoodId.HasValue)
            {
                var neighbourhoodExists = await _neighbourhoodRepository.NeighbourhoodExists(updatePropertyDto.neighbourhoodId.Value);
                if (!neighbourhoodExists)
                    throw new KeyNotFoundException($"Neighbourhood with id {updatePropertyDto.neighbourhoodId.Value} not found");
                property.neighbourhoodId = updatePropertyDto.neighbourhoodId.Value;
            }

            property.updatedAt = DateTime.UtcNow;
            await _propertyRepository.Update(property);
            await SyncPropertyPolygonAsync(property.id, property.ownerId, property.coordinates);
            await WriteLogAsync(
                property.ownerId,
                Log.OperationType.Update,
                "Property updated",
                $"Property #{property.id} '{property.title}' was updated.",
                userIP);
        }

        public async Task DeletePropertyAsync(int id, string userIP = "")
        {
            var property = await _propertyRepository.GetById(id);
            if (property == null)
                throw new KeyNotFoundException($"Property with id {id} not found");

            await _propertyRepository.Delete(id);
            await _polygonService.DeletePolygonByLabelAsync(GetPropertyPolygonLabel(id), property.ownerId);
            await WriteLogAsync(
                property.ownerId,
                Log.OperationType.Delete,
                "Property deleted",
                $"Property #{property.id} '{property.title}' was deleted.",
                userIP);
        }

        public async Task DeletePropertiesAsync(IEnumerable<int> ids, string userIP = "")
        {
            foreach (var id in ids.Distinct())
            {
                await DeletePropertyAsync(id, userIP);
            }
        }

        public async Task<bool> PropertyExistsAsync(int id)
        {
            return await _propertyRepository.Exists(id);
        }

        public async Task<int> GetPropertyCountByUserAsync(int userId)
        {
            return await _propertyRepository.GetPropertyCountByUser(userId);
        }

        private PropertyDTO MapToPropertyDTO(Property property)
        {
            return new PropertyDTO
            {
                propertyID = property.id,
                title = property.title,
                description = property.description,
                price = property.price,
                address = property.address,
                isReadOnly = property.isReadOnly,
                ownerId = property.ownerId,
                ownerName = property.owner != null ? $"{property.owner.firstname} {property.owner.lastname}" : string.Empty,
                ownerEmail = property.owner?.email ?? string.Empty,
                parcelNumber = property.parcelNumber,
                lotNumber = property.lotNumber,
                area = property.area,
                coordinates = property.coordinates,
                imagePath = property.imagePath,
                bedrooms = property.bedrooms,
                bathrooms = property.bathrooms,
                parkingSpaces = property.parkingSpaces,
                propertyType = property.type,
                createdAt = property.createdAt,
                updatedAt = property.updatedAt,
                neighbourhoodId = property.neighbourhoodId,
                neighbourhoodName = property.neighbourhood?.name ?? string.Empty,
                districtName = property.neighbourhood?.district?.districtName ?? string.Empty,
                cityName = property.neighbourhood?.district?.city?.cityName ?? string.Empty
            };
        }

        private async Task SyncPropertyPolygonAsync(int propertyId, int ownerId, string coordinates)
        {
            if (string.IsNullOrWhiteSpace(coordinates)) return;
            await _polygonService.UpsertPolygonByLabelAsync(GetPropertyPolygonLabel(propertyId), coordinates, ownerId);
        }

        private static string GetPropertyPolygonLabel(int propertyId) => $"PROPERTY-{propertyId}";

        private async Task WriteLogAsync(
            int userId,
            Log.OperationType operationType,
            string message,
            string description,
            string userIP)
        {
            await _logRepository.Add(new Log
            {
                userId = userId,
                operationType = operationType,
                message = message,
                description = description,
                status = Log.Status.Completed,
                userIP = userIP,
                createdAt = DateTime.UtcNow
            });
        }

        private static void ValidateCreateProperty(CreatePropertyDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.title) ||
                string.IsNullOrWhiteSpace(dto.description) ||
                string.IsNullOrWhiteSpace(dto.address) ||
                string.IsNullOrWhiteSpace(dto.parcelNumber) ||
                string.IsNullOrWhiteSpace(dto.lotNumber) ||
                string.IsNullOrWhiteSpace(dto.coordinates) ||
                dto.price <= 0 ||
                dto.area <= 0 ||
                dto.ownerId <= 0 ||
                dto.neighbourhoodId <= 0)
            {
                throw new InvalidOperationException("Please fill in all required fields with valid format.");
            }
        }

        private static void ValidateUpdateProperty(UpdatePropertyDTO dto)
        {
            var isImageOnlyUpdate =
                !string.IsNullOrWhiteSpace(dto.imagePath) &&
                string.IsNullOrWhiteSpace(dto.title) &&
                string.IsNullOrWhiteSpace(dto.description) &&
                string.IsNullOrWhiteSpace(dto.address) &&
                string.IsNullOrWhiteSpace(dto.parcelNumber) &&
                string.IsNullOrWhiteSpace(dto.lotNumber) &&
                string.IsNullOrWhiteSpace(dto.coordinates) &&
                !dto.price.HasValue &&
                !dto.area.HasValue &&
                !dto.bedrooms.HasValue &&
                !dto.bathrooms.HasValue &&
                !dto.parkingSpaces.HasValue &&
                !dto.neighbourhoodId.HasValue;

            if (isImageOnlyUpdate)
                return;

            if (string.IsNullOrWhiteSpace(dto.title) ||
                string.IsNullOrWhiteSpace(dto.description) ||
                string.IsNullOrWhiteSpace(dto.address) ||
                string.IsNullOrWhiteSpace(dto.parcelNumber) ||
                string.IsNullOrWhiteSpace(dto.lotNumber) ||
                string.IsNullOrWhiteSpace(dto.coordinates) ||
                !dto.price.HasValue ||
                dto.price <= 0 ||
                !dto.area.HasValue ||
                dto.area <= 0 ||
                !dto.neighbourhoodId.HasValue ||
                dto.neighbourhoodId <= 0)
            {
                throw new InvalidOperationException("Please enter valid property details.");
            }
        }
    }
}
