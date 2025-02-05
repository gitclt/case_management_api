using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_subadminController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_subadminController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Postsubadmin")]

        public async Task<ActionResult> Postsubadmin([FromBody] case_login request)
        {
            if (_context.tbl_case_login == null)
            {
                return Problem("Entity set '_context.tbl_case_login'  is null.");
            }
            var division = new case_login
            {
                name = request.name,
                username = request.username,
                password = request.password,
                contact_person = request.contact_person,
                mobile = request.mobile,
                account_id = request.account_id,
                active_status = request.active_status,
                type = "subadmin",
                delete_status = 0
            };


            _context.tbl_case_login.Add(division);
            await _context.SaveChangesAsync();

            // return Ok("successfully");

            return Ok(new { status = true, message = "Data added successfully" });
        }

        [HttpPut]
        [Route("update_subadmin")]
        public async Task<ActionResult> update_subadmin([FromBody] case_login request)
        {
            var data = await _context.tbl_case_login.FindAsync(request.id);

            if (data == null)
            {
                return Ok(new { status = false, message = "subadmin record not found" });
            }


            if (request.name != null)
            {
                data.name = request.name;
            }

            if (request.username != null)
            {
                data.username = request.username;
            }
            if (request.password != null)
            {
                data.password = request.password;
            }
            if (request.contact_person != null)
            {
                data.contact_person = request.contact_person;
            }
            if (request.mobile != null)
            {
                data.mobile = request.mobile;
            }
            if (request.active_status != null)
            {
                data.active_status = request.active_status;
            }



            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }


        [HttpDelete]
        [Route("delete_subadmin")]

        public async Task<IActionResult> delete_subadmin([FromForm] int? id)
        {
            if (_context.tbl_case_login == null)
            {
                return Problem("Entity set '_context.tbl_case_login' is null.");
            }

            var div = await _context.tbl_case_login.FindAsync(id);
 
            if (div == null)
            {
                return NotFound(new { status = false, message = "subadmin not found" });
            }


            div.delete_status = 1;

            var subadmins = _context.tbl_case_subadminassembly.Where(x => x.subadmin_id == id);
            _context.tbl_case_subadminassembly.RemoveRange(subadmins);

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

        [HttpGet]
        [Route("get_subadmin")]

        public async Task<IActionResult> get_subadmin(int? id,int? account_id)
        {
            if (_context.tbl_case_login == null)
            {
                return Problem("Entity set '_context.tbl_case_login' is null.");
            }

            var query = _context.tbl_case_login
                .Where(c => c.delete_status == 0 && (!id.HasValue || c.id == id.Value) && c.type == "subadmin")
                .Select(c => new
                {
                    id = c.id.ToString(),
                    name = c.name,
                    account_id = c.account_id,
                    username = c.username,
                    password = c.password,
                    contact_person = c.contact_person,
                    mobile = c.mobile,
                    assemblies = (
                        from a in _context.tbl_case_subadminassembly
                        join ca in _context.tbl_case_assembly on a.assembly_id equals ca.id
                        where a.subadmin_id == c.id 
                        select new
                        {
                            assembly_id = ca.id.ToString(),
                            assembly = ca.name
                        }
                    ).ToList()
                })
                .ToList();
           
            // Check if no results were found
            if (query == null || query.Count == 0)
            {
                return NotFound(new { status = false, message = "No subadmin found" });
            }

            // Return the response with the required structure
            return Ok(new
            {
                status = true,
                message = "Success.",
                data = query
            });
        }

    }
}
