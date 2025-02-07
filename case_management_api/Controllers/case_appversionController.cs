using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_appversionController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_appversionController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("Getappversionmodel")]
        public async Task<ActionResult<IEnumerable<appversionmodel>>> Getappversionmodel()
        {
            if (_context.tbl_case_app_version == null)
            {
                return NotFound();
            }

            var appversionmodels = await _context.tbl_case_app_version.ToListAsync();

            if (appversionmodels == null || !appversionmodels.Any())
            {
                return NotFound();
            }

            return Ok(appversionmodels);
        }


        [HttpPut]
        [Route("update_appversion")]
        public async Task<IActionResult> update_appversion([FromForm] int? id, [FromForm] string? version_name, [FromForm] string? version_code)
        {
            if (id == null)
            {
                return BadRequest(new { status = false, message = "ID is required" });
            }

            var order = await _context.tbl_case_app_version.FindAsync(id);

            if (order == null)
            {
                return NotFound(new { status = false, message = "Record not found" });
            }

            // Update FCM token if provided
            if (!string.IsNullOrEmpty(version_name))
            {
                order.version_name = version_name;
            }
            if (!string.IsNullOrEmpty(version_code))
            {
                order.version_code = version_code;
            }

            _context.tbl_case_app_version.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }



    }
}
