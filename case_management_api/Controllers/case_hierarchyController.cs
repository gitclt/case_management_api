using api_case_management.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_hierarchyController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_hierarchyController(case_managementDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [Route("get_hierarchy")]
        public async Task<IActionResult> get_hierarchy(int? id)
        {
            if (_context.tbl_case_hierarchy == null)
            {
                return Problem("Entity set '_context.tbl_case_hierarchy' is null.");
            }

            // If id is provided, get specific designation, otherwise get all designations
            var roleQuery = _context.tbl_case_hierarchy
                                            .Where(d => d.delete_status == 0);
            if (id.HasValue)
            {
                roleQuery = roleQuery.Where(g => g.id == id.Value); // Fix: retrieve the specific id
            }
          
            var query = from c in roleQuery

                        where c.delete_status == 0
                        select new
                        {
                            c.id,
                            c.name,
                           
                            c.account_id

                        };



            return Ok(new
            {
                status = true,
                Message = "Success.",
                data = query
            });
        }

    }
}
