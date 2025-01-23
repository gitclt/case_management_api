using System.Security.Cryptography;
using api_case_management.Data;
using Azure.Core;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_casesController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_casesController(case_managementDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("Postcases")]
        public async Task<ActionResult> Postcases([FromBody] case_cases request)
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases'  is null.");
            }

            var division = new case_cases
            {
                account_id = request.account_id,
                type = request.type,
                name = request.name,
                address = request.address,
                email = request.email,
                mobile = request.mobile,
                location = request.location,
                category_id = request.category_id,
                priority_id = request.priority_id,
                assembly_id = request.assembly_id,
                date = request.date,
                time = request.time,
                title = request.title,
                comment = request.comment,
                description = request.description,
                delete_status = 0,
                status = "pending",
                addedon = DateTime.Now,
                addedby = request.addedby,
                addedtype = request.addedtype,
                reminder_date = request.reminder_date,
            };

            _context.tbl_case_cases.Add(division);
            await _context.SaveChangesAsync();

            var caseid = division.id;

            //insertion to order status
         
            var history = new case_status
            {
                case_id = caseid,
               addedon=DateTime.Now,    
                status = "pending",
              //  remark = "Order placed",

            };

            _context.tbl_case_status.Add(history);

            await _context.SaveChangesAsync();
            return Ok(new { status = true, message = "Data added successfully" });
        }

        [HttpPut]
        [Route("Updatecases")]
        public async Task<ActionResult> Updatecases(case_cases request)
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            if (request.id.HasValue)
            {
                return BadRequest(new { status = false, message = "Employee ID is required" });
            }

            var case_id = request.id.Value;

            var existingEmployee = await _context.tbl_case_cases.FindAsync(case_id);
            if (existingEmployee == null)
            {
                return NotFound(new { status = false, message = "cases not found" });
            }
            if (!string.IsNullOrEmpty(request.type))
            {
                existingEmployee.type = request.type;
            }
            if (!string.IsNullOrEmpty(request.name))
            {
                existingEmployee.name = request.name;
            }
            if (!string.IsNullOrEmpty(request.address))
            {
                existingEmployee.address = request.address;
            }
            if (!string.IsNullOrEmpty(request.email))
            {
                existingEmployee.email = request.email;
            }
            if (!string.IsNullOrEmpty(request.mobile))
            {
                existingEmployee.mobile = request.mobile;
            }
            if (!string.IsNullOrEmpty(request.location))
            {
                existingEmployee.location = request.location;
            }
            if (request.category_id.HasValue)
            {
                existingEmployee.category_id = request.category_id;
            }
            if (request.priority_id.HasValue)
            {
                existingEmployee.priority_id = request.priority_id.Value;
            }

            if (request.assembly_id.HasValue)
            {
                existingEmployee.assembly_id = request.assembly_id.Value;
            }
            if (!string.IsNullOrEmpty(request.title))
            {
                existingEmployee.title = request.title;
            }
            if (!string.IsNullOrEmpty(request.comment))
            {
                existingEmployee.comment = request.comment;
            }
            if (request.date.HasValue)
            {
                existingEmployee.date = request.date;
            }
            if (!string.IsNullOrEmpty(request.description))
            {
                existingEmployee.description = request.description;
            }
            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }

        [HttpDelete]
        [Route("delete_cases")]

        public async Task<IActionResult> delete_cases([FromForm] int id)
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            var div = await _context.tbl_case_cases.FindAsync(id);

            if (div == null)
            {
                return NotFound(new { status = false, message = "cases not found" });
            }

            // Set delete_status to 1 for soft delete
            div.delete_status = 1;

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

        [HttpGet]
        [Route("view_cases")]
        public async Task<IActionResult> view_cases(int? id, int? category_id, int? priority_id, int? account_id, string? keyword, string? type, DateTime? fromdate, DateTime? todate, string? month, int page = 1, int pageSize = 10)
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            // Base query for cases with delete_status = 0
            var empqry = _context.tbl_case_cases
                                 .Where(e => e.delete_status == 0);

            if (id.HasValue)
            {
                empqry = empqry.Where(e => e.id == id.Value);
            }
            if (category_id.HasValue && category_id > 0)
            {
                empqry = empqry.Where(e => e.category_id == category_id.Value);
            }
            if (priority_id.HasValue && priority_id > 0)
            {
                empqry = empqry.Where(e => e.priority_id == priority_id.Value);
            }
            //if (assembly_id.HasValue && assembly_id > 0)
            //{
            //    empqry = empqry.Where(e => e.assembly_id == assembly_id.Value);
            //}
            if (account_id.HasValue && account_id > 0)
            {
                empqry = empqry.Where(e => e.account_id == account_id.Value);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                empqry = empqry.Where(c => c.name.Contains(keyword) || c.address == keyword || c.location.Contains(keyword));
            }

            if (fromdate != null && todate != null)
            {
                empqry = empqry.Where(c =>
                    (fromdate == null || c.date >= fromdate) &&
                    (todate == null || c.date <= todate));
            }

            //for all activity list
            if (month != null)
            {
                empqry = empqry.Where(c => c.date.HasValue && c.date.Value.Month == int.Parse(month));

            }


            if (!string.IsNullOrWhiteSpace(type))
            {
                empqry = empqry.Where(e => e.type == type);
            }

            // Build the query
            var query = from c in empqry
                        join s in _context.tbl_case_category on c.category_id equals s.id
                        join d in _context.tbl_case_priority on c.priority_id equals d.id
                        join p in _context.tbl_case_assembly on c.assembly_id equals p.id
                        where s.delete_status == 0 && d.delete_status == 0
                        select new
                        {
                            c.id,
                            c.name,
                            c.address,
                            c.email,
                            c.mobile,
                            c.location,
                            c.category_id,
                            category = s.name,
                            c.priority_id,
                            c.assembly_id,
                            assembly = p.name,
                            priority = d.name,
                            c.time,
                            c.date,
                            c.title,
                            c.description,
                            c.status,
                            c.type,
                            contact_person = (
                                from ca in _context.tbl_case_contactperson
                                where ca.case_id == c.id
                                select new
                                {
                                    ca.id,
                                    contact_person = ca.name.ToString(),
                                    ca.mobile
                                }
                            ).ToList() // Convert subquery to a list
                        };

            if (!await query.AnyAsync())
            {
                return NotFound(new { status = false, message = "No cases found" });
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var resultList = await query
                                 .Skip((page - 1) * pageSize)
                                 .Take(pageSize)
                                 .ToListAsync();

            // Filter and format response based on the `type` parameter
            object dataResponse;

            if (type == "support request")
            {
                dataResponse = resultList.Select(result => new
                {
                    result.id,
                    result.name,
                    result.email,
                    result.mobile,
                    result.title,
                    result.description,
                    result.date,
                    result.priority_id,
                    result.priority,
                    //app 
                    result.assembly_id,
                    result.assembly,
                    result.category_id,
                    result.category,
                    result.status,

                }).ToList();
            }
            else if (type == "program schedule")
            {
                dataResponse = resultList.Select(result => new
                {
                    result.id,
                    //  result.name,
                    result.location,
                    result.title,
                    result.description,
                    result.mobile,
                    result.date,
                    result.time,
                    result.priority_id,
                    result.priority,
                    result.category_id,
                    result.contact_person

                }).ToList();
            }
            else
            {
                // Default response for unknown or unspecified types
                dataResponse = resultList.Select(result => new
                {
                    result.id,

                    result.location,
                    result.priority_id,
                    result.priority,
                    result.assembly_id,
                    result.assembly,
                    result.date,
                    result.time,
                    result.title,
                    result.description,
                    result.status,
                    result.contact_person

                }).ToList();
            }

            return Ok(new
            {
                status = true,
                message = "Success.",
                totalCount,
                totalPages,
                data = dataResponse
            });
        }

        [HttpGet]
        [Route("cases_detail")]
        public async Task<IActionResult> cases_detail(int? id, string? type, int? account_id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/";

            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            // Base query for cases with delete_status = 0
            var empqry = _context.tbl_case_cases
                                 .Where(e => e.delete_status == 0);

            if (id.HasValue)
            {
                empqry = empqry.Where(e => e.id == id.Value);
            }
            if (account_id.HasValue && account_id > 0)
            {
                empqry = empqry.Where(e => e.account_id == account_id.Value);
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                empqry = empqry.Where(e => e.type == type); // Filter by type
            }

            var query = from c in empqry
                        join s in _context.tbl_case_category on c.category_id equals s.id
                        join d in _context.tbl_case_priority on c.priority_id equals d.id
                        join p in _context.tbl_case_assembly on c.assembly_id equals p.id
                        where s.delete_status == 0 && d.delete_status == 0
                        select new
                        {
                            c.id,
                            c.name,
                            c.address,
                            c.email,
                            c.mobile,
                            c.location,
                            c.category_id,
                            category = s.name,
                            c.priority_id,
                            c.assembly_id,
                            priority = d.name,
                            c.time,
                            c.date,
                            c.title,
                            c.description,
                            c.type,
                            c.status,
                            //case documents
                            case_documents = (
                                from cd in _context.tbl_case_documents
                                where cd.case_id == c.id
                                select new
                                {
                                    cd.id,
                                    document_path = !string.IsNullOrEmpty(cd.document)
                                ? $"{baseUrl}uploads/{c.type}/{c.id}/{cd.document}"
                                : null
                                }
                            ).ToList(), // Convert subquery to a list

                            //case status
                            case_status = (
                                from cd in _context.tbl_case_status
                                where cd.case_id == c.id
                                select new
                                {
                                    cd.id,
                                    cd.remark,
                                    cd.status,
                                    cd.addedon,
                                    cd.case_id

                                }
                            ).ToList(), // Convert subquery to a list


                            //contact person
                            contact_person = (
                                from ca in _context.tbl_case_contactperson
                                where ca.case_id == c.id
                                select new
                                {
                                    ca.id,
                                    contact_person = ca.name.ToString(),
                                    ca.mobile
                                }
                            ).ToList() // Convert subquery to a list
                        };

            if (!await query.AnyAsync())
            {
                return NotFound(new { status = false, message = "No cases found" });
            }


            // Filter and format response based on the `type` parameter
            object dataResponse;

            if (type == "support request")
            {
                dataResponse = query.Select(result => new
                {
                    result.id,
                    result.name,
                    result.email,
                    result.mobile,
                    result.title,
                    result.description,
                    result.category_id,
                    result.category,
                    result.date,
                    result.priority_id,
                    result.priority,
                    result.case_documents,
                    //result.contact_person,
                    result.status,
                    result.case_status

                }).ToList();
            }
            else if (type == "program schedule")
            {
                dataResponse = query.Select(result => new
                {
                    result.id,
                    result.location,
                    result.title,
                    result.name,
                    result.description,
                    result.mobile,
                    result.date,
                    //result.time,
                    result.priority_id,
                    result.priority,
                    result.category_id,
                    result.category,
                    result.case_documents,
                    result.contact_person,
                    result.status,
                    result.case_status


                }).ToList();
            }
            else
            {
                // Default response for unknown or unspecified types
                dataResponse = query.Select(result => new
                {
                    result.id,

                    result.location,
                    result.category_id,
                    result.priority_id,
                    result.priority,
                    result.assembly_id,
                    result.date,
                    result.title,
                    result.description,
                    result.status,

                    result.case_documents,
                    result.contact_person,
                    result.case_status


                }).ToList();
            }

            return Ok(new
            {
                status = true,
                message = "Success.",

                data = dataResponse
            });
        }

        [HttpPut]
        [Route("update_status")]
        public async Task<IActionResult> update_status([FromForm] int? id, [FromForm] string? status, [FromForm] string? remark, [FromForm] DateTime? reminder_date, [FromForm] IFormFile? document)
        {
            if (id == null)
            {
                return BadRequest(new { status = false, message = "ID is required" });
            }

            var order = await _context.tbl_case_cases.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { status = false, message = "Case not found" });
            }

            // Update the order details
            order.status = status;
            order.reminder_date = reminder_date;
            await _context.SaveChangesAsync();

            // Handle document upload
            string? fileName = null;
            if (document != null && document.Length > 0)
            {
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/uploads/cases/{id}");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                fileName = Path.GetFileName(document.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }
            }

            // Add case status history
            var history = new case_status
            {
                case_id = id.Value,
                status = status,
                remark = remark,
                addedon = DateTime.Now,
                file = fileName // Save the uploaded document's file name
               
            };
            await _context.tbl_case_status.AddAsync(history);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }

    }
}
