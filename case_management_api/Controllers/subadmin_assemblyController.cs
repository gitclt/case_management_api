using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class subadmin_assemblyController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public subadmin_assemblyController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Post_subadmin_assembly")]
        public async Task<ActionResult> Post_subadmin_assembly([FromBody] List<case_subadmin_assembly> requests)
        {
            if (_context.tbl_case_subadminassembly == null)
            {
                return Problem("Entity set 'case_managementDbContext.tbl_case_subadminassembly' is null.");
            }

            if (requests == null || !requests.Any())
            {
                return BadRequest(new { status = false, message = "No data provided." });
            }

            var divisions = requests.Select(request => new case_subadmin_assembly
            {
                subadmin_id = request.subadmin_id,
                assembly_id = request.assembly_id,
                account_id = request.account_id
            }).ToList();

            _context.tbl_case_subadminassembly.AddRange(divisions);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data added successfully." });
        }


        [HttpDelete]
        [Route("delete_subadmin_assembly")]
        public async Task<IActionResult> delete_subadmin_assembly(int id)
        {

            if (_context.tbl_case_subadminassembly == null)
            {
                return Problem("Entity set '_context.tbl_case_subadminassembly' is null.");
            }

            var div = await _context.tbl_case_subadminassembly.FindAsync(id);
            // return Ok(div);

            if (div == null)
            {
                return NotFound(new { status = false, message = "not found" });
            }
            _context.tbl_case_subadminassembly.Remove(div);

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

    }
}
