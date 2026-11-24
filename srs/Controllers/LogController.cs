using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using srs.DTO;
using srs.Interfaces;

namespace srs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogDTO>>> getAllLogs()
        {
            var logs = await _logService.GetAllLogsAsync();
            return Ok(logs);
        }

        [HttpGet("view")]
        public async Task<ActionResult<IEnumerable<LogViewDTO>>> getAllLogsView()
        {
            var logs = await _logService.GetAllLogsViewAsync();
            return Ok(logs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LogDTO>> getLogById(int id)
        {
            var log = await _logService.GetLogByIdAsync(id);
            if (log == null)
                return NotFound(new { message = $"Log with id {id} not found" });

            return Ok(log);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<LogDTO>>> getLogsByUser(int userId)
        {
            var logs = await _logService.GetLogsByUserAsync(userId);
            return Ok(logs);
        }

        [HttpGet("daterange")]
        public async Task<ActionResult<IEnumerable<LogDTO>>> getLogsByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var logs = await _logService.GetLogsByDateRangeAsync(startDate, endDate);
            return Ok(logs);
        }

        [HttpPost("filter")]
        public async Task<ActionResult<IEnumerable<LogDTO>>> getFilteredLogs([FromBody] FilterLogDTO filterDto)
        {
            var logs = await _logService.GetFilteredLogsAsync(filterDto);
            return Ok(logs);
        }

        [HttpPost]
        public async Task<ActionResult<LogDTO>> createLog([FromBody] CreateLogDTO createLogDto)
        {
            try
            {
                var log = await _logService.CreateLogAsync(createLogDto);
                return CreatedAtAction(nameof(getLogById), new { id = log.id }, log);
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
        public async Task<IActionResult> deleteLog(int id)
        {
            try
            {
                await _logService.DeleteLogAsync(id);
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

        // ── EXPORT ──────────────────────────────────────────────────────────────

        [HttpGet("export/excel")]
        public async Task<IActionResult> exportToExcel()
        {
            var logs = await _logService.GetAllLogsViewAsync();
            return ExportLogsToExcel(logs);
        }

        [HttpPost("export/excel")]
        public async Task<IActionResult> exportFilteredToExcel([FromBody] FilterLogDTO filterDto)
        {
            var logs = await GetExportLogs(filterDto);
            return ExportLogsToExcel(logs);
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> exportToPdf()
        {
            var logs = await _logService.GetAllLogsViewAsync();
            return ExportLogsToPdf(logs);
        }

        [HttpPost("export/pdf")]
        public async Task<IActionResult> exportFilteredToPdf([FromBody] FilterLogDTO filterDto)
        {
            var logs = await GetExportLogs(filterDto);
            return ExportLogsToPdf(logs);
        }

        private async Task<IEnumerable<LogViewDTO>> GetExportLogs(FilterLogDTO filterDto)
        {
            var hasFilters = filterDto.userId.HasValue ||
                filterDto.operationType.HasValue ||
                filterDto.status.HasValue ||
                !string.IsNullOrWhiteSpace(filterDto.description) ||
                !string.IsNullOrWhiteSpace(filterDto.userIP) ||
                filterDto.startDate.HasValue ||
                filterDto.endDate.HasValue;

            if (!hasFilters)
                return await _logService.GetAllLogsViewAsync();

            filterDto.page = 1;
            filterDto.pageSize = int.MaxValue;
            var logs = await _logService.GetFilteredLogsAsync(filterDto);
            return logs.Select(l => new LogViewDTO
            {
                id = l.id,
                userName = l.userName,
                userId = l.userId,
                operationType = l.operationType,
                message = l.message,
                description = l.description,
                status = l.status,
                userIP = l.userIP,
                timestamp = l.createdAt
            });
        }

        private IActionResult ExportLogsToExcel(IEnumerable<LogViewDTO> logs)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Logs");
            string[] headers = ["ID", "User", "User ID", "Operation", "Message", "Description", "Status", "User IP", "Timestamp"];
            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
                sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(26, 35, 126));
                sheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            int row = 2;
            foreach (var l in logs)
            {
                sheet.Cells[row, 1].Value = l.id;
                sheet.Cells[row, 2].Value = l.userName;
                sheet.Cells[row, 3].Value = l.userId;
                sheet.Cells[row, 4].Value = l.operationType.ToString();
                sheet.Cells[row, 5].Value = l.message;
                sheet.Cells[row, 6].Value = l.description;
                sheet.Cells[row, 7].Value = l.status.ToString();
                sheet.Cells[row, 8].Value = l.userIP;
                sheet.Cells[row, 9].Value = l.timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                row++;
            }

            sheet.Cells.AutoFitColumns();
            sheet.View.FreezePanes(2, 1);
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "logs.xlsx");
        }

        private IActionResult ExportLogsToPdf(IEnumerable<LogViewDTO> logs)
        {
            using var stream = new MemoryStream();
            using (var writer = new PdfWriter(stream))
            using (var pdf = new PdfDocument(writer))
            using (var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate()))
            {
                document.SetMargins(24, 18, 24, 18);
                document.Add(new Paragraph("Logs Report").SetFontSize(16));
                document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}").SetFontSize(9));

                var table = new Table(UnitValue.CreatePercentArray([0.55f, 1.4f, 0.65f, 1f, 1.4f, 2.1f, 0.9f, 1f, 1.3f]))
                    .UseAllAvailableWidth()
                    .SetFontSize(7);

                string[] headers = ["ID", "User", "User ID", "Operation", "Message", "Description", "Status", "User IP", "Timestamp"];
                foreach (var header in headers)
                    table.AddHeaderCell(new Cell().Add(new Paragraph(header)));

                foreach (var l in logs)
                {
                    table.AddCell(l.id.ToString());
                    table.AddCell(l.userName);
                    table.AddCell(l.userId.ToString());
                    table.AddCell(l.operationType.ToString());
                    table.AddCell(l.message);
                    table.AddCell(l.description);
                    table.AddCell(l.status.ToString());
                    table.AddCell(l.userIP);
                    table.AddCell(l.timestamp.ToString("yyyy-MM-dd HH:mm"));
                }

                document.Add(table);
            }

            return File(stream.ToArray(), "application/pdf", "logs.pdf");
        }
    }
}
