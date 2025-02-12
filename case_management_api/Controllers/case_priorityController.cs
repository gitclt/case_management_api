﻿using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_priorityController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_priorityController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Postpriority")]

        public async Task<ActionResult> Postpriority([FromBody] case_priority request)
        {
            if (_context.tbl_case_priority == null)
            {
                return Problem("Entity set 'case_managementDbContext.tbl_case_priority'  is null.");
            }
            var division = new case_priority
            {
                name = request.name,
                account_id = request.account_id,
                delete_status = 0,
                addedby= request.addedby,   
                addedon=DateTime.Now,   
            };


            _context.tbl_case_priority.Add(division);
            await _context.SaveChangesAsync();

            // return Ok("successfully");

            return Ok(new { status = true, message = "Data added successfully" });
        }

        [HttpPut]
        [Route("update_priority")]
        public async Task<ActionResult> update_priority([FromBody] case_priority request)
        {
            var data = await _context.tbl_case_priority.FindAsync(request.id);

            if (data == null)
            {
                return Ok(new { status = false, message = "Priority record not found" });
            }


            if (request.name != null)
            {
                data.name = request.name;
            }

            if (request.account_id != null)
            { 
                data.account_id = request.account_id;
            }
            if (request.modifiedby != null)
            {
                data.modifiedby = request.modifiedby;
            }
            request.modifiedon= DateTime.Now;   

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }


        [HttpDelete]
        [Route("delete_priority")]

        public async Task<IActionResult> delete_priority([FromForm] int id, [FromForm] int? deletedby)
        {
            if (_context.tbl_case_priority == null)
            {
                return Problem("Entity set '_context.tbl_case_priority' is null.");
            }

            var div = await _context.tbl_case_priority.FindAsync(id);

            if (div == null)
            {
                return NotFound(new { status = false, message = "priority not found" });
            }

            bool hasActiveCases = await _context.tbl_case_cases.AnyAsync(c => c.priority_id == id && c.delete_status == 0);

            if (hasActiveCases)
            {
                return BadRequest(new { status = false, message = "Cannot delete priority as active cases exist under this priority." });
            }


            // Set delete_status to 1 for soft delete
            div.delete_status = 1;
            div.deletedon= DateTime.Now;
            div.deletedby = deletedby;
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

        [HttpGet]
        [Route("get_priority")]

        public async Task<IActionResult> get_priority(int? id,int? account_id)
        {
            if (_context.tbl_case_priority == null)
            {
                return Problem("Entity set '_context.tbl_case_priority' is null.");
            }

            // If id is provided, get specific designation, otherwise get all designations
            var divisionQuery = _context.tbl_case_priority
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
                return NotFound(new { status = false, message = "No priority found" });
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
