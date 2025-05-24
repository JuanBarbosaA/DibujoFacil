using backend.Dtos;
using backend.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;

        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetAllUsers(adminId));
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UserUpdateDto userDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.UpdateUser(adminId, id, userDto));
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _adminService.GetAllRoles());
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.DeleteUser(adminId, id));
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AdminUserCreationDto userDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.CreateUser(adminId, userDto));
        }

        [HttpGet("tutorials")]
        public async Task<IActionResult> GetAllTutorials()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetAllTutorials(adminId));
        }

        [HttpPut("tutorials/{id}/contents")]
        public async Task<IActionResult> UpdateTutorialContents(int id, [FromForm] TutorialContentUpdateDto contentDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.UpdateTutorialContents(adminId, id, contentDto));
        }

        [HttpDelete("tutorials/{id}")]
        public async Task<IActionResult> DeleteTutorial(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.DeleteTutorial(adminId, id));
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _adminService.GetAllCategories());
        }

        [HttpGet("tutorials/pending")]
        public async Task<IActionResult> GetPendingTutorials()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetPendingTutorials(adminId));
        }

        [HttpPut("tutorials/{id}/approve")]
        public async Task<IActionResult> ApproveTutorial(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.ApproveTutorial(adminId, id));
        }

        [HttpGet("users/statistics/export")]
        public async Task<IActionResult> ExportUserStatistics()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var stats = await _adminService.GetUserStatistics(adminId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Estadísticas de Usuarios");

            // Encabezados
            worksheet.Cell(1, 1).Value = "ID Usuario";
            worksheet.Cell(1, 2).Value = "Nombre";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Tutoriales";
            worksheet.Cell(1, 5).Value = "Comentarios";
            worksheet.Cell(1, 6).Value = "Rating Promedio";

            // Estilo encabezados
            var headerRange = worksheet.Range("A1:F1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            // Datos
            int row = 2;
            foreach (var stat in stats)
            {
                worksheet.Cell(row, 1).Value = stat.UserId;
                worksheet.Cell(row, 2).Value = stat.Name;
                worksheet.Cell(row, 3).Value = stat.Email;
                worksheet.Cell(row, 4).Value = stat.TutorialCount;
                worksheet.Cell(row, 5).Value = stat.CommentCount;
                worksheet.Cell(row, 6).Value = stat.AverageRating;
                row++;
            }

            // Autoajustar columnas
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "UserStatistics.xlsx");
        }
    }
    }