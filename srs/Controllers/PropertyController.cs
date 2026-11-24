using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using srs.DTO;
using srs.Interfaces;

namespace srs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        private readonly IUserRepository _userRepository;
        private readonly INeighbourhoodRepository _neighbourhoodRepository;

        public PropertyController(
            IPropertyService propertyService,
            IUserRepository userRepository,
            INeighbourhoodRepository neighbourhoodRepository)
        {
            _propertyService = propertyService;
            _userRepository = userRepository;
            _neighbourhoodRepository = neighbourhoodRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PropertyDTO>>> getAllProperties()
        {
            var properties = await _propertyService.GetAllPropertiesAsync();
            return Ok(properties);
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<PropertyViewDTO>>> getAllPropertiesView()
        {
            var properties = await _propertyService.GetAllPropertiesViewAsync();
            return Ok(properties);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PropertyDTO>> getPropertyById(int id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null)
                return NotFound(new { message = $"Property with id {id} not found" });

            return Ok(property);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PropertyViewDTO>>> getPropertiesByUser(int userId)
        {
            var properties = await _propertyService.GetPropertiesByUserAsync(userId);
            return Ok(properties);
        }

        [HttpGet("neighbourhood/{neighbourhoodId}")]
        public async Task<ActionResult<IEnumerable<PropertyViewDTO>>> getPropertiesByNeighbourhood(int neighbourhoodId)
        {
            var properties = await _propertyService.GetPropertiesByNeighbourhoodAsync(neighbourhoodId);
            return Ok(properties);
        }

        [HttpGet("district/{districtId}")]
        public async Task<ActionResult<IEnumerable<PropertyViewDTO>>> getPropertiesByDistrict(int districtId)
        {
            var properties = await _propertyService.GetPropertiesByDistrictAsync(districtId);
            return Ok(properties);
        }

        [HttpGet("city/{cityId}")]
        public async Task<ActionResult<IEnumerable<PropertyViewDTO>>> getPropertiesByCity(int cityId)
        {
            var properties = await _propertyService.GetPropertiesByCityAsync(cityId);
            return Ok(properties);
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<PropertyDTO>>> searchProperties([FromBody] SearchPropertyDTO searchDto)
        {
            var properties = await _propertyService.SearchPropertiesAsync(searchDto);
            return Ok(properties);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PropertyDTO>> createProperty([FromBody] CreatePropertyDTO createPropertyDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!IsRegularUser() || currentUserId == null || createPropertyDto.ownerId != currentUserId.Value)
                    return Forbid();

                var property = await _propertyService.CreatePropertyAsync(createPropertyDto, GetClientIP());
                return CreatedAtAction(nameof(getPropertyById), new { id = property.propertyID }, property);
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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> updateProperty(int id, [FromBody] UpdatePropertyDTO updatePropertyDto)
        {
            try
            {
                var existing = await _propertyService.GetPropertyByIdAsync(id);
                var currentUserId = GetCurrentUserId();
                if (existing == null)
                    return NotFound(new { message = $"Property with id {id} not found" });
                if (!IsRegularUser() || currentUserId == null || existing.ownerId != currentUserId.Value)
                    return Forbid();
                if (existing.isReadOnly)
                    return BadRequest(new { message = "This property is read only and cannot be edited." });

                await _propertyService.UpdatePropertyAsync(id, updatePropertyDto, GetClientIP());
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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteProperty(int id)
        {
            try
            {
                var existing = await _propertyService.GetPropertyByIdAsync(id);
                var currentUserId = GetCurrentUserId();
                if (existing == null)
                    return NotFound(new { message = $"Property with id {id} not found" });
                if (!IsRegularUser() || currentUserId == null || existing.ownerId != currentUserId.Value)
                    return Forbid();

                await _propertyService.DeletePropertyAsync(id, GetClientIP());
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

        [Authorize]
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> bulkDeleteProperties([FromBody] BulkDeletePropertyDTO dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!IsRegularUser() || currentUserId == null)
                    return Forbid();

                var ids = dto.propertyIds.Distinct().ToList();
                foreach (var id in ids)
                {
                    var existing = await _propertyService.GetPropertyByIdAsync(id);
                    if (existing == null)
                        return NotFound(new { message = $"Property with id {id} not found" });
                    if (existing.ownerId != currentUserId.Value)
                        return Forbid();
                }

                await _propertyService.DeletePropertiesAsync(ids, GetClientIP());
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> propertyExists(int id)
        {
            var exists = await _propertyService.PropertyExistsAsync(id);
            return Ok(new { exists });
        }

        [HttpGet("user/{userId}/count")]
        public async Task<ActionResult<int>> getPropertyCountByUser(int userId)
        {
            var count = await _propertyService.GetPropertyCountByUserAsync(userId);
            return Ok(new { count });
        }

        // ── EXPORT ──────────────────────────────────────────────────────────────

        [Authorize]
        [HttpGet("export/excel")]
        public async Task<IActionResult> exportToExcel([FromQuery] SearchPropertyDTO filters)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var properties = await GetExportProperties(filters);

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Properties");
            string[] headers = ["ID", "Title", "Parcel No", "Lot No", "Address", "City", "District", "Neighbourhood", "Type", "Area (m²)", "Coordinates", "Image Path", "Price", "Bedrooms", "Bathrooms", "Parking", "Owner", "Owner Email", "Created At"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
                sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(26, 35, 126));
                sheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }
            int row = 2;
            foreach (var p in properties)
            {
                sheet.Cells[row, 1].Value = p.propertyID;
                sheet.Cells[row, 2].Value = p.title;
                sheet.Cells[row, 3].Value = p.parcelNumber;
                sheet.Cells[row, 4].Value = p.lotNumber;
                sheet.Cells[row, 5].Value = p.address;
                sheet.Cells[row, 6].Value = p.cityName;
                sheet.Cells[row, 7].Value = p.districtName;
                sheet.Cells[row, 8].Value = p.neighbourhoodName;
                sheet.Cells[row, 9].Value = p.propertyType.ToString();
                sheet.Cells[row, 10].Value = p.area;
                sheet.Cells[row, 11].Value = p.coordinates;
                sheet.Cells[row, 12].Value = p.imagePath;
                sheet.Cells[row, 13].Value = p.price;
                sheet.Cells[row, 14].Value = p.bedrooms;
                sheet.Cells[row, 15].Value = p.bathrooms;
                sheet.Cells[row, 16].Value = p.parkingSpaces;
                sheet.Cells[row, 17].Value = p.ownerName;
                sheet.Cells[row, 18].Value = p.ownerEmail;
                sheet.Cells[row, 19].Value = p.createdAt.ToString("yyyy-MM-dd");
                row++;
            }
            sheet.Cells.AutoFitColumns();
            sheet.View.FreezePanes(2, 1);
            var bytes = package.GetAsByteArray();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "properties.xlsx");
        }

        [Authorize]
        [HttpGet("export/pdf")]
        public async Task<IActionResult> exportToPdf([FromQuery] SearchPropertyDTO filters)
        {
            var properties = await GetExportProperties(filters);

            using var stream = new MemoryStream();
            using (var writer = new PdfWriter(stream))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate()))
            {
                document.SetMargins(24, 18, 24, 18);
                document.Add(new Paragraph("Properties Report").SetFontSize(16));
                document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").SetFontSize(9));

                var table = new Table(UnitValue.CreatePercentArray([0.7f, 1.7f, 1f, 1f, 2.2f, 1.2f, 1.2f, 1.1f, 0.8f, 1f, 1.4f]))
                    .UseAllAvailableWidth()
                    .SetFontSize(7);

                string[] headers = ["ID", "Title", "Parcel", "Lot", "Address", "City", "District", "Type", "Area", "Price", "Owner"];
                foreach (var header in headers)
                    table.AddHeaderCell(new Cell().Add(new Paragraph(header)));

                foreach (var p in properties)
                {
                    table.AddCell(p.propertyID.ToString());
                    table.AddCell(p.title);
                    table.AddCell(p.parcelNumber);
                    table.AddCell(p.lotNumber);
                    table.AddCell(p.address);
                    table.AddCell(p.cityName);
                    table.AddCell(p.districtName);
                    table.AddCell(p.propertyType.ToString());
                    table.AddCell(p.area.ToString("F2"));
                    table.AddCell(p.price.ToString("F2"));
                    table.AddCell(p.ownerName);
                }

                document.Add(table);
            }

            return File(stream.ToArray(), "application/pdf", "properties.pdf");
        }

        private async Task<List<PropertyDTO>> GetExportProperties(SearchPropertyDTO filters)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return [];

            if (!User.IsInRole("Admin"))
                filters.ownerId = currentUserId.Value;

            var properties = await _propertyService.SearchPropertiesAsync(filters);
            return properties
                .OrderBy(p => p.createdAt)
                .ThenBy(p => p.propertyID)
                .ToList();
        }

        // ── IMPORT ──────────────────────────────────────────────────────────────

        [HttpPost("import/excel")]
        public async Task<IActionResult> importFromExcel(IFormFile file, [FromQuery] int userId)
        {
            if (file == null || !file.FileName.EndsWith(".xlsx"))
                return BadRequest(new { message = "Import failed. Please check the file format and data." });

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var imported = new List<PropertyDTO>();
            try
            {
                using var stream = file.OpenReadStream();
                using var package = new ExcelPackage(stream);
                var sheet = package.Workbook.Worksheets[0];
                int rows = sheet.Dimension?.Rows ?? 0;
                var headers = BuildHeaderMap(sheet);
                var requiredHeaders = new[]
                {
                    "title", "description", "price", "address", "ownername",
                    "parcelnumber", "lotnumber", "coordinates", "bedrooms",
                    "bathrooms", "parkingspaces", "propertytype", "city",
                    "district", "neighbourhood"
                };

                if (rows < 2 || requiredHeaders.Any(header => !headers.ContainsKey(header)))
                    return BadRequest(new { message = "Import failed. Please check the file format and data." });

                for (int r = 2; r <= rows; r++)
                {
                    if (IsEmptyRow(sheet, r))
                        continue;

                    var ownerName = GetCell(sheet, headers, r, "ownername");
                    var owner = await _userRepository.GetUserByNameOrEmail(ownerName);
                    if (owner == null)
                        return BadRequest(new { message = $"Import failed. Owner '{ownerName}' was not found." });

                    var neighbourhood = await _neighbourhoodRepository.GetByLocationNames(
                        GetCell(sheet, headers, r, "city"),
                        GetCell(sheet, headers, r, "district"),
                        GetCell(sheet, headers, r, "neighbourhood"));

                    if (neighbourhood == null)
                        return BadRequest(new { message = $"Import failed. Location on row {r} was not found." });

                    var coordinates = GetCell(sheet, headers, r, "coordinates");
                    var area = CalculateAreaFromCoordinates(coordinates);
                    if (area <= 0)
                        return BadRequest(new { message = $"Import failed. Coordinates on row {r} must be a valid GeoJSON polygon." });

                    if (!TryParseDecimal(GetCell(sheet, headers, r, "price"), out var price) ||
                        !TryParseInt(GetCell(sheet, headers, r, "bedrooms"), out var bedrooms) ||
                        !TryParseInt(GetCell(sheet, headers, r, "bathrooms"), out var bathrooms) ||
                        !TryParseInt(GetCell(sheet, headers, r, "parkingspaces"), out var parkingSpaces))
                    {
                        return BadRequest(new { message = $"Import failed. Numeric values on row {r} are invalid." });
                    }

                    var createDto = new CreatePropertyDTO
                    {
                        title = GetCell(sheet, headers, r, "title"),
                        description = GetCell(sheet, headers, r, "description"),
                        price = price,
                        address = GetCell(sheet, headers, r, "address"),
                        propertyType = Enum.TryParse<srs.Entity.Property.PropertyType>(GetCell(sheet, headers, r, "propertytype"), true, out var pt)
                            ? pt
                            : srs.Entity.Property.PropertyType.Land,
                        area = area,
                        coordinates = coordinates,
                        imagePath = string.Empty,
                        ownerId = owner.id,
                        parcelNumber = GetCell(sheet, headers, r, "parcelnumber"),
                        lotNumber = GetCell(sheet, headers, r, "lotnumber"),
                        bedrooms = bedrooms,
                        bathrooms = bathrooms,
                        parkingSpaces = parkingSpaces,
                        neighbourhoodId = neighbourhood.id
                    };

                    if (string.IsNullOrWhiteSpace(createDto.title) ||
                        string.IsNullOrWhiteSpace(createDto.description) ||
                        string.IsNullOrWhiteSpace(createDto.address) ||
                        string.IsNullOrWhiteSpace(createDto.parcelNumber) ||
                        string.IsNullOrWhiteSpace(createDto.lotNumber) ||
                        createDto.price <= 0)
                    {
                        return BadRequest(new { message = "Import failed. Please check the file format and data." });
                    }

                    var created = await _propertyService.CreatePropertyAsync(createDto, GetClientIP());
                    imported.Add(created);
                }
            }
            catch
            {
                return BadRequest(new { message = "Import failed. Please check the file format and data." });
            }
            return Ok(new { message = "Properties imported successfully.", count = imported.Count });
        }

        private static Dictionary<string, int> BuildHeaderMap(ExcelWorksheet sheet)
        {
            var headers = new Dictionary<string, int>();
            int columns = sheet.Dimension?.Columns ?? 0;
            for (int c = 1; c <= columns; c++)
            {
                var header = NormalizeHeader(sheet.Cells[1, c].Text);
                if (!string.IsNullOrWhiteSpace(header) && !headers.ContainsKey(header))
                    headers[header] = c;
            }

            return headers;
        }

        private static string NormalizeHeader(string value)
        {
            var normalized = string.Concat(value
                .Trim()
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit));

            return normalized switch
            {
                "owner" => "ownername",
                "ownerfullname" => "ownername",
                "parking" => "parkingspaces",
                "parkingspace" => "parkingspaces",
                "neighborhood" => "neighbourhood",
                "neighborhoodname" => "neighbourhood",
                "neighbourhoodname" => "neighbourhood",
                "parcelno" => "parcelnumber",
                "lotno" => "lotnumber",
                "type" => "propertytype",
                _ => normalized
            };
        }

        private static string GetCell(ExcelWorksheet sheet, Dictionary<string, int> headers, int row, string header)
        {
            return headers.TryGetValue(header, out var column)
                ? sheet.Cells[row, column].Text.Trim()
                : string.Empty;
        }

        private static bool IsEmptyRow(ExcelWorksheet sheet, int row)
        {
            int columns = sheet.Dimension?.Columns ?? 0;
            for (int c = 1; c <= columns; c++)
            {
                if (!string.IsNullOrWhiteSpace(sheet.Cells[row, c].Text))
                    return false;
            }

            return true;
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            var clean = value.Replace("₺", string.Empty).Trim();
            return decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out result) ||
                   decimal.TryParse(clean, NumberStyles.Number, CultureInfo.CurrentCulture, out result);
        }

        private static bool TryParseInt(string value, out int result)
        {
            return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result) ||
                   int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out result);
        }

        private static double CalculateAreaFromCoordinates(string coordinates)
        {
            var points = ReadGeoJsonPolygon(coordinates);
            if (points.Count == 0)
                points = ReadCoordinateRing(coordinates);

            if (points.Count < 4)
                return 0;

            var averageLatitude = points.Average(p => p.Latitude) * Math.PI / 180d;
            const double earthRadius = 6378137d;
            double area = 0;

            for (int i = 0; i < points.Count; i++)
            {
                var current = Project(points[i], averageLatitude, earthRadius);
                var next = Project(points[(i + 1) % points.Count], averageLatitude, earthRadius);
                area += current.X * next.Y - next.X * current.Y;
            }

            return Math.Round(Math.Abs(area) / 2d, 2);
        }

        private static List<(double Longitude, double Latitude)> ReadGeoJsonPolygon(string coordinates)
        {
            try
            {
                using var document = JsonDocument.Parse(coordinates);
                var root = document.RootElement;
                if (!root.TryGetProperty("type", out var type) ||
                    !string.Equals(type.GetString(), "Polygon", StringComparison.OrdinalIgnoreCase) ||
                    !root.TryGetProperty("coordinates", out var coordinateGroups) ||
                    coordinateGroups.ValueKind != JsonValueKind.Array ||
                    coordinateGroups.GetArrayLength() == 0)
                {
                    return [];
                }

                var ring = coordinateGroups[0];
                if (ring.ValueKind != JsonValueKind.Array)
                    return [];

                return ReadJsonRing(ring);
            }
            catch
            {
                return [];
            }
        }

        private static List<(double Longitude, double Latitude)> ReadJsonRing(JsonElement ring)
        {
            var points = new List<(double Longitude, double Latitude)>();
            foreach (var point in ring.EnumerateArray())
            {
                if (point.ValueKind != JsonValueKind.Array || point.GetArrayLength() < 2)
                    return [];

                points.Add((point[0].GetDouble(), point[1].GetDouble()));
            }

            return points;
        }

        private static List<(double Longitude, double Latitude)> ReadCoordinateRing(string coordinates)
        {
            var matches = Regex.Matches(coordinates, @"-?\d+(?:[.,]\d+)?");
            if (matches.Count < 8 || matches.Count % 2 != 0)
                return [];

            var points = new List<(double Longitude, double Latitude)>();
            for (int i = 0; i < matches.Count; i += 2)
            {
                if (!TryParseDouble(matches[i].Value, out var longitude) ||
                    !TryParseDouble(matches[i + 1].Value, out var latitude))
                {
                    return [];
                }

                points.Add((longitude, latitude));
            }

            return points;
        }

        private static bool TryParseDouble(string value, out double result)
        {
            var clean = value.Trim().Replace(',', '.');
            return double.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        private static (double X, double Y) Project((double Longitude, double Latitude) point, double averageLatitude, double earthRadius)
        {
            var longitude = point.Longitude * Math.PI / 180d;
            var latitude = point.Latitude * Math.PI / 180d;
            return (earthRadius * longitude * Math.Cos(averageLatitude), earthRadius * latitude);
        }

        // ── IMAGE UPLOAD ─────────────────────────────────────────────────────────

        [Authorize]
        [HttpPost("{id}/upload-image")]
        public async Task<IActionResult> uploadImage(int id, IFormFile image)
        {
            var existing = await _propertyService.GetPropertyByIdAsync(id);
            var currentUserId = GetCurrentUserId();
            if (existing == null)
                return NotFound(new { message = $"Property with id {id} not found" });
            if (!IsRegularUser() || currentUserId == null || existing.ownerId != currentUserId.Value)
                return Forbid();
            if (existing.isReadOnly)
                return BadRequest(new { message = "This property is read only and cannot be edited." });

            if (image == null) return BadRequest(new { message = "No file provided." });
            var ext = Path.GetExtension(image.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                return BadRequest(new { message = "Only JPEG and PNG images are supported." });
            if (image.Length > 100 * 1024 * 1024)
                return BadRequest(new { message = "File size must not exceed 100 MB." });

            var uploadsDir = OperatingSystem.IsWindows()
                ? Path.Combine("C:\\Uploads")
                : Path.Combine(Path.GetTempPath(), "srs-uploads");
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{id}_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var fs = new FileStream(filePath, FileMode.Create))
                await image.CopyToAsync(fs);

            var imagePath = $"/uploads/{fileName}";
            var updateDto = new UpdatePropertyDTO { imagePath = imagePath };
            await _propertyService.UpdatePropertyAsync(id, updateDto, GetClientIP());
            return Ok(new { imagePath });
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        private bool IsRegularUser() => User.IsInRole("User");

        private string GetClientIP()
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                ?? string.Empty;
        }
    }
}
