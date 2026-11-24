using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using srs.Entity;

namespace srs.Data;

public static class TurkeyAdministrativeDataSeeder
{
    private const string SeedFileName = "turkey-administrative-divisions.json";
    private const string NeighbourhoodSeedFileName = "turkey-neighbourhoods.json";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        var seedPath = Path.Combine(environment.ContentRootPath, "Data", SeedFileName);
        if (!File.Exists(seedPath))
            return;

        var seedData = await ReadSeedDataAsync(seedPath);
        if (seedData.Count == 0)
            return;

        var existingCities = await context.cities.ToListAsync();
        var citiesById = existingCities.ToDictionary(c => c.id);
        var citiesByName = existingCities.ToDictionary(c => c.cityName, StringComparer.OrdinalIgnoreCase);

        foreach (var seedCity in seedData)
        {
            if (citiesById.TryGetValue(seedCity.Id, out var city) ||
                citiesByName.TryGetValue(seedCity.Name, out city))
            {
                if (city.cityName != seedCity.Name)
                    city.cityName = seedCity.Name;
            }
            else
            {
                city = new City { id = seedCity.Id, cityName = seedCity.Name };
                context.cities.Add(city);
                citiesById[city.id] = city;
                citiesByName[city.cityName] = city;
            }
        }

        await context.SaveChangesAsync();

        existingCities = await context.cities.ToListAsync();
        citiesById = existingCities.ToDictionary(c => c.id);
        citiesByName = existingCities.ToDictionary(c => c.cityName, StringComparer.OrdinalIgnoreCase);

        var existingDistricts = await context.districts.ToListAsync();
        var districtsById = existingDistricts.ToDictionary(d => d.id);
        var districtKeys = existingDistricts
            .Select(d => DistrictKey(d.cityId, d.districtName))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seedCity in seedData)
        {
            if (!citiesById.TryGetValue(seedCity.Id, out var city) &&
                !citiesByName.TryGetValue(seedCity.Name, out city))
            {
                continue;
            }

            foreach (var seedDistrict in seedCity.Districts)
            {
                var key = DistrictKey(city.id, seedDistrict.Name);
                if (districtsById.TryGetValue(seedDistrict.Id, out var district))
                {
                    district.districtName = seedDistrict.Name;
                    district.cityId = city.id;
                    if (string.IsNullOrWhiteSpace(district.postalCode))
                        district.postalCode = string.Empty;

                    districtKeys.Add(key);
                    continue;
                }

                if (districtKeys.Contains(key))
                    continue;

                context.districts.Add(new District
                {
                    id = seedDistrict.Id,
                    districtName = seedDistrict.Name,
                    cityId = city.id,
                    postalCode = string.Empty
                });
                districtKeys.Add(key);
            }
        }

        await context.SaveChangesAsync();
        await ResetIdentitySequencesAsync(context);

        await SeedNeighbourhoodsAsync(context, environment);
    }

    private static async Task<List<SeedCity>> ReadSeedDataAsync(string seedPath)
    {
        await using var stream = File.OpenRead(seedPath);
        var seedRoot = await JsonSerializer.DeserializeAsync<SeedRoot>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return seedRoot?.Data ?? [];
    }

    private static async Task ResetIdentitySequencesAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """SELECT setval(pg_get_serial_sequence('"cities"', 'id'), COALESCE((SELECT MAX("id") FROM "cities"), 1));""");
        await context.Database.ExecuteSqlRawAsync(
            """SELECT setval(pg_get_serial_sequence('"districts"', 'id'), COALESCE((SELECT MAX("id") FROM "districts"), 1));""");
        await context.Database.ExecuteSqlRawAsync(
            """SELECT setval(pg_get_serial_sequence('"neighbourhoods"', 'id'), COALESCE((SELECT MAX("id") FROM "neighbourhoods"), 1));""");
    }

    private static string DistrictKey(int cityId, string districtName) => $"{cityId}:{districtName}";
    private static string NeighbourhoodKey(int districtId, string neighbourhoodName) => $"{districtId}:{neighbourhoodName}";

    private static async Task SeedNeighbourhoodsAsync(AppDbContext context, IHostEnvironment environment)
    {
        var seedPath = Path.Combine(environment.ContentRootPath, "Data", NeighbourhoodSeedFileName);
        if (!File.Exists(seedPath))
            return;

        var seedData = await ReadNeighbourhoodSeedDataAsync(seedPath);
        if (seedData.Count == 0)
            return;

        var cities = await context.cities.ToListAsync();
        var citiesByName = cities.ToDictionary(c => c.cityName, StringComparer.OrdinalIgnoreCase);

        var districts = await context.districts.ToListAsync();
        var districtsByCityAndName = districts
            .ToDictionary(d => DistrictKey(d.cityId, d.districtName), StringComparer.OrdinalIgnoreCase);

        var neighbourhoodKeys = (await context.neighbourhoods.ToListAsync())
            .Select(n => NeighbourhoodKey(n.districtId, n.name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var seedCity in seedData)
        {
            if (!citiesByName.TryGetValue(seedCity.City, out var city))
                continue;

            foreach (var seedDistrict in seedCity.Districts)
            {
                if (!districtsByCityAndName.TryGetValue(DistrictKey(city.id, seedDistrict.Name), out var district))
                    continue;

                foreach (var neighbourhoodName in seedDistrict.Neighbourhoods)
                {
                    if (string.IsNullOrWhiteSpace(neighbourhoodName))
                        continue;

                    var key = NeighbourhoodKey(district.id, neighbourhoodName);
                    if (neighbourhoodKeys.Contains(key))
                        continue;

                    context.neighbourhoods.Add(new Neighbourhood
                    {
                        name = neighbourhoodName,
                        districtId = district.id
                    });
                    neighbourhoodKeys.Add(key);
                }
            }
        }

        await context.SaveChangesAsync();
        await ResetIdentitySequencesAsync(context);
    }

    private static async Task<List<NeighbourhoodSeedCity>> ReadNeighbourhoodSeedDataAsync(string seedPath)
    {
        await using var stream = File.OpenRead(seedPath);
        var seedRoot = await JsonSerializer.DeserializeAsync<NeighbourhoodSeedRoot>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return seedRoot?.Data ?? [];
    }

    private sealed class SeedRoot
    {
        public List<SeedCity> Data { get; set; } = [];
    }

    private sealed class SeedCity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<SeedDistrict> Districts { get; set; } = [];
    }

    private sealed class SeedDistrict
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class NeighbourhoodSeedRoot
    {
        public List<NeighbourhoodSeedCity> Data { get; set; } = [];
    }

    private sealed class NeighbourhoodSeedCity
    {
        public string City { get; set; } = string.Empty;
        public List<NeighbourhoodSeedDistrict> Districts { get; set; } = [];
    }

    private sealed class NeighbourhoodSeedDistrict
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Neighbourhoods { get; set; } = [];
    }
}
