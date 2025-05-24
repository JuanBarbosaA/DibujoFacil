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

        /// <summary>
        /// Obtiene todos los usuarios.
        /// </summary>
        /// <returns>Lista de usuarios.</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetAllUsers(adminId));
        }

        /// <summary>
        /// Edita un usuario existente.
        /// </summary>
        /// <param name="id">ID del usuario a editar.</param>
        /// <param name="userDto">Datos para actualizar el usuario.</param>
        /// <returns>Resultado de la actualización.</returns>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> EditUser(int id, [FromBody] UserUpdateDto userDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.UpdateUser(adminId, id, userDto));
        }

        /// <summary>
        /// Obtiene todos los roles disponibles.
        /// </summary>
        /// <returns>Lista de roles.</returns>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(await _adminService.GetAllRoles());
        }

        /// <summary>
        /// Elimina un usuario.
        /// </summary>
        /// <param name="id">ID del usuario a eliminar.</param>
        /// <returns>Resultado de la eliminación.</returns>
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.DeleteUser(adminId, id));
        }

        /// <summary>
        /// Crea un nuevo usuario administrador.
        /// </summary>
        /// <param name="userDto">Datos del usuario a crear.</param>
        /// <returns>Resultado de la creación.</returns>
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] AdminUserCreationDto userDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.CreateUser(adminId, userDto));
        }

        /// <summary>
        /// Obtiene todos los tutoriales.
        /// </summary>
        /// <returns>Lista de tutoriales.</returns>
        [HttpGet("tutorials")]
        public async Task<IActionResult> GetAllTutorials()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetAllTutorials(adminId));
        }

        /// <summary>
        /// Actualiza los contenidos de un tutorial.
        /// </summary>
        /// <param name="id">ID del tutorial a actualizar.</param>
        /// <param name="contentDto">Datos del contenido a actualizar.</param>
        /// <returns>Resultado de la actualización.</returns>
        [HttpPut("tutorials/{id}/contents")]
        public async Task<IActionResult> UpdateTutorialContents(int id, [FromForm] TutorialContentUpdateDto contentDto)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.UpdateTutorialContents(adminId, id, contentDto));
        }

        /// <summary>
        /// Elimina un tutorial.
        /// </summary>
        /// <param name="id">ID del tutorial a eliminar.</param>
        /// <returns>Resultado de la eliminación.</returns>
        [HttpDelete("tutorials/{id}")]
        public async Task<IActionResult> DeleteTutorial(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.DeleteTutorial(adminId, id));
        }

        /// <summary>
        /// Obtiene todas las categorías.
        /// </summary>
        /// <returns>Lista de categorías.</returns>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _adminService.GetAllCategories());
        }

        /// <summary>
        /// Obtiene los tutoriales pendientes por aprobar.
        /// </summary>
        /// <returns>Lista de tutoriales pendientes.</returns>
        [HttpGet("tutorials/pending")]
        public async Task<IActionResult> GetPendingTutorials()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.GetPendingTutorials(adminId));
        }

        /// <summary>
        /// Aprueba un tutorial pendiente.
        /// </summary>
        /// <param name="id">ID del tutorial a aprobar.</param>
        /// <returns>Resultado de la aprobación.</returns>
        [HttpPut("tutorials/{id}/approve")]
        public async Task<IActionResult> ApproveTutorial(int id)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            return Ok(await _adminService.ApproveTutorial(adminId, id));
        }

        /// <summary>
        /// Exporta las estadísticas de los usuarios en un archivo Excel.
        /// </summary>
        /// <returns>Archivo Excel con estadísticas.</returns>
        [HttpGet("users/statistics/export")]
        public async Task<IActionResult> ExportUserStatistics()
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var stats = await _adminService.GetUserStatistics(adminId);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Estadísticas de Usuarios");

            worksheet.Cell(1, 1).Value = "ID Usuario";
            worksheet.Cell(1, 2).Value = "Nombre";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Tutoriales";
            worksheet.Cell(1, 5).Value = "Comentarios";
            worksheet.Cell(1, 6).Value = "Rating Promedio";

            var headerRange = worksheet.Range("A1:F1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

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
