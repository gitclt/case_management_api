using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_contactpersonController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_contactpersonController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Post_case_contactperson")]
        public async Task<ActionResult> Post_case_contactperson([FromBody] List<case_contactperson> requests)
        {
            if (_context.tbl_case_contactperson == null)
            {
                return Problem("Entity set '_context.tbl_case_contactperson' is null.");
            }

            if (requests == null || !requests.Any())
            {
                return BadRequest(new { status = false, message = "No data provided." });
            }

            var divisions = requests.Select(request => new case_contactperson
            {
                type = request.type,    
                case_id = request.case_id,
                name = request.name,
                account_id = request.account_id,
                designation = request.designation,
                mobile = request.mobile,
            }).ToList();

            _context.tbl_case_contactperson.AddRange(divisions);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data added successfully." });
        }

    }
}
