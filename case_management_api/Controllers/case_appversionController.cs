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

    }
}
