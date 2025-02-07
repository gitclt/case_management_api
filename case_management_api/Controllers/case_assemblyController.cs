﻿using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_assemblyController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_assemblyController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Postassembly")]

        public async Task<ActionResult> Postassembly([FromBody] case_assembly request)
        {
            if (_context.tbl_case_assembly == null)
            {
                return Problem("Entity set '_context.tbl_case_assembly'  is null.");
            }
            var division = new case_assembly
            {
                name = request.name,
                account_id = request.account_id,
                delete_status = 0,
                addedby= request.addedby,   
                addedon=DateTime.Now,
                
            };


            _context.tbl_case_assembly.Add(division);
            await _context.SaveChangesAsync();

            // return Ok("successfully");

            return Ok(new { status = true, message = "Data added successfully" });
        }

        [HttpPut]
        [Route("update_assembly")]
        public async Task<ActionResult> update_assembly([FromBody] case_assembly request)
        {
            var data = await _context.tbl_case_assembly.FindAsync(request.id);

            if (data == null)
            {
                return Ok(new { status = false, message = "assembly record not found" });
            }


            if (request.name != null)
            {
                data.name = request.name;
            }

            if (request.account_id != null)
            { data.account_id = request.account_id; }
            if (request.modifiedby != null)
            {
                data.modifiedby = request.modifiedby;
            }
            if (request.modifiedon != null)
            {
                data.modifiedon = DateTime.Now;
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }


        [HttpDelete]
        [Route("delete_assembly")]

        public async Task<IActionResult> delete_assembly([FromForm] int id, [FromForm] int? deletedby)
        {
            if (_context.tbl_case_assembly == null)
            {
                return Problem("Entity set '_context.tbl_case_assembly' is null.");
            }

            var div = await _context.tbl_case_assembly.FindAsync(id);

            if (div == null)
            {
                return NotFound(new { status = false, message = "assembly not found" });
            }

            bool hasActiveCases = await _context.tbl_case_cases.AnyAsync(c => c.assembly_id == id && c.delete_status == 0);

            if (hasActiveCases)
            {
                return BadRequest(new { status = false, message = "Cannot delete assembly as active cases exist under this assembly." });
            }


            // Set delete_status to 1 for soft delete
            div.delete_status = 1;
            div.deletedby = deletedby;
            div.deletedon = DateTime.Now;       

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

        [HttpGet]
        [Route("get_assembly")]

        public async Task<IActionResult> get_assembly(int? id,int? account_id)
        {
            if (_context.tbl_case_assembly == null)
            {
                return Problem("Entity set '_context.tbl_case_assembly' is null.");
            }

            // If id is provided, get specific designation, otherwise get all designations
            var divisionQuery = _context.tbl_case_assembly
                                            .Where(d => d.delete_status == 0);

            if (id.HasValue)
            {
                divisionQuery = divisionQuery.Where(d => d.id == id.Value);
            }
            if (account_id.HasValue)
            {
                divisionQuery = divisionQuery.Where(d => d.account_id == account_id.Value);
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
                return NotFound(new { status = false, message = "No assembly found" });
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
