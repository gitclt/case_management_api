using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_roleController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_roleController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Postrole")]

        public async Task<ActionResult> Postrole([FromBody] case_role request)
        {
            if (_context.tbl_case_role == null)
            {
                return Problem("Entity set '_context.tbl_case_role'  is null.");
            }
            var division = new case_role
            {
                name = request.name,
                account_id = request.account_id,
                hierarchy_id = request.hierarchy_id,    
                delete_status = 0
            };


            _context.tbl_case_role.Add(division);
            await _context.SaveChangesAsync();

            // return Ok("successfully");

            return Ok(new { status = true, message = "Data added successfully" });
        }

        [HttpPut]
        [Route("update_role")]
        public async Task<ActionResult> update_role([FromBody] case_role request)
        {
            var data = await _context.tbl_case_role.FindAsync(request.id);

            if (data == null)
            {
                return Ok(new { status = false, message = "role record not found" });
            }


            if (request.name != null)
            {
                data.name = request.name;
            }

            if (request.account_id != null)
            { 
                data.account_id = request.account_id; 
            }
            if (request.hierarchy_id != null)
            {
                data.hierarchy_id = request.hierarchy_id;
            }


            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }


        [HttpDelete]
        [Route("delete_role")]

        public async Task<IActionResult> delete_role([FromForm] int id)
        {
            if (_context.tbl_case_role == null)
            {
                return Problem("Entity set '_context.tbl_case_role' is null.");
            }

            var div = await _context.tbl_case_role.FindAsync(id);

            if (div == null)
            {
                return NotFound(new { status = false, message = "role not found" });
            }

            // Set delete_status to 1 for soft delete
            div.delete_status = 1;

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

        [HttpGet]
        [Route("get_role")]
        public async Task<IActionResult> get_role(int? id,int? hierarchy_id)
        {
            if (_context.tbl_case_role == null)
            {
                return Problem("Entity set '_context.tbl_case_role' is null.");
            }
            
            // If id is provided, get specific designation, otherwise get all designations
            var roleQuery = _context.tbl_case_role
                                            .Where(d => d.delete_status == 0);
            if (id.HasValue)
            {
                roleQuery = roleQuery.Where(g => g.id == id.Value); // Fix: retrieve the specific id
            }
            if (hierarchy_id.HasValue)
            {
                roleQuery = roleQuery.Where(g => g.hierarchy_id == hierarchy_id.Value); // Fix: retrieve the specific id
            }

            var query = from c in roleQuery
                        join h in _context.tbl_case_hierarchy on c.hierarchy_id equals h.id into hi

                        from pr in hi.DefaultIfEmpty()
                        where (c == null || c.delete_status == 0) && (pr==null|| pr.delete_status == 0)

                        select new
                        {
                            c.id,
                            c.name,
                            c.hierarchy_id,
                            c.account_id,
                            hierarchy = pr.name,

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
