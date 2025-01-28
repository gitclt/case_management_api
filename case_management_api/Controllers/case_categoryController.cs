using api_case_management.Data;
using api_case_management.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_categoryController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_categoryController(case_managementDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("Postcategory")]

        public async Task<ActionResult> Postcategory([FromBody] case_category request)
        {
            if (_context.tbl_case_category == null)
            {
                return Problem("Entity set 'case_managementDbContext.tbl_case_category'  is null.");
            }
            var division = new case_category
            {
                name = request.name,
                account_id = request.account_id,
                delete_status = 0
            };


            _context.tbl_case_category.Add(division);
            await _context.SaveChangesAsync();

            // return Ok("successfully");

            return Ok(new { status = true, message = "Data added successfully" });
        }

        [HttpPut]
        [Route("update_category")]
        public async Task<ActionResult> update_category([FromBody] case_category request)
        {
            var data = await _context.tbl_case_category.FindAsync(request.id);

            if (data == null)
            {
                return Ok(new { status = false, message = "Category record not found" });
            }


            if (request.name != null)
            {
                data.name = request.name;
            }

            if (request.account_id != null)
            { data.account_id = request.account_id; }


            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }


        [HttpDelete]
        [Route("delete_category")]
        public async Task<IActionResult> delete_category([FromForm] int id)
        {
            if (_context.tbl_case_category == null)
            {
                return Problem("Entity set '_context.tbl_case_category' is null.");
            }

            var div = await _context.tbl_case_category.FindAsync(id);

            if (div == null)
            {
                return NotFound(new { status = false, message = "category not found" });
            }

            // Set delete_status to 1 for soft delete
            div.delete_status = 1;

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

        [HttpGet]
        [Route("get_category")]

        public async Task<IActionResult> get_category(int? id)
        {
            if (_context.tbl_case_category == null)
            {
                return Problem("Entity set '_context.tbl_case_category' is null.");
            }

            // If id is provided, get specific designation, otherwise get all designations
            var divisionQuery = _context.tbl_case_category
                                            .Where(d => d.delete_status == 0);

            if (id.HasValue)
            {
                divisionQuery = divisionQuery.Where(d => d.id == id.Value);
            }

            var cat = divisionQuery
                                            .Select(d => new
                                            {
                                                id = d.id.ToString(),       // Convert id to string if needed
                                                name = d.name,
                                                account_id = d.account_id,
                                            })
                                            .ToList();

            if (cat == null || cat.Count == 0)
            {
                return NotFound(new { status = false, message = "No category found" });
            }

            return Ok(new
            {
                status = true,
                Message = "Success.",
                data = cat
            });
        }

    }
}
