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
                subject = request.subject,      
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
        //public async Task<ActionResult> Updatecases(case_cases request)
        //{
        //    if (_context.tbl_case_cases == null)
        //    {
        //        return Problem("Entity set '_context.tbl_case_cases' is null.");
        //    }

        //    if (request.id.HasValue)
        //    {
        //        return BadRequest(new { status = false, message = "ID is required" });
        //    }

        //    var case_id = request.id.Value;

        //    var existingEmployee = await _context.tbl_case_cases.FindAsync(case_id);
        //    if (existingEmployee == null)
        //    {
        //        return NotFound(new { status = false, message = "cases not found" });
        //    }
        //    if (!string.IsNullOrEmpty(request.type))
        //    {
        //        existingEmployee.type = request.type;
        //    }
        //    if (!string.IsNullOrEmpty(request.name))
        //    {
        //        existingEmployee.name = request.name;
        //    }
        //    if (!string.IsNullOrEmpty(request.address))
        //    {
        //        existingEmployee.address = request.address;
        //    }
        //    if (!string.IsNullOrEmpty(request.email))
        //    {
        //        existingEmployee.email = request.email;
        //    }
        //    if (!string.IsNullOrEmpty(request.mobile))
        //    {
        //        existingEmployee.mobile = request.mobile;
        //    }
        //    if (!string.IsNullOrEmpty(request.location))
        //    {
        //        existingEmployee.location = request.location;
        //    }
        //    if (request.category_id.HasValue)
        //    {
        //        existingEmployee.category_id = request.category_id;
        //    }
        //    if (request.priority_id.HasValue)
        //    {
        //        existingEmployee.priority_id = request.priority_id.Value;
        //    }

        //    if (request.assembly_id.HasValue)
        //    {
        //        existingEmployee.assembly_id = request.assembly_id.Value;
        //    }
        //    if (!string.IsNullOrEmpty(request.title))
        //    {
        //        existingEmployee.title = request.title;
        //    }
        //    if (!string.IsNullOrEmpty(request.comment))
        //    {
        //        existingEmployee.comment = request.comment;
        //    }
        //    if (request.date.HasValue)
        //    {
        //        existingEmployee.date = request.date;
        //    }
        //    if (!string.IsNullOrEmpty(request.description))
        //    {
        //        existingEmployee.description = request.description;
        //    }
        //    // Save changes to the database
        //    await _context.SaveChangesAsync();

        //    return Ok(new { status = true, message = "Data updated successfully" });
        //}
        public async Task<ActionResult> Updatecases(case_cases request)
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            if (!request.id.HasValue) // Fixing the ID validation
            {
                return BadRequest(new { status = false, message = "ID is required" });
            }

            var case_id = request.id.Value;
            var existingCase = await _context.tbl_case_cases.FindAsync(case_id);

            if (existingCase == null)
            {
                return NotFound(new { status = false, message = "Case not found" });
            }

            // Updating fields only if they are provided in the request
            existingCase.type = !string.IsNullOrEmpty(request.type) ? request.type : existingCase.type;
            existingCase.name = !string.IsNullOrEmpty(request.name) ? request.name : existingCase.name;
            existingCase.address = !string.IsNullOrEmpty(request.address) ? request.address : existingCase.address;
            existingCase.email = !string.IsNullOrEmpty(request.email) ? request.email : existingCase.email;
            existingCase.mobile = !string.IsNullOrEmpty(request.mobile) ? request.mobile : existingCase.mobile;
            existingCase.location = !string.IsNullOrEmpty(request.location) ? request.location : existingCase.location;
            existingCase.category_id = request.category_id ?? existingCase.category_id;
            existingCase.priority_id = request.priority_id ?? existingCase.priority_id;
            existingCase.assembly_id = request.assembly_id ?? existingCase.assembly_id;
            existingCase.title = !string.IsNullOrEmpty(request.title) ? request.title : existingCase.title;
            existingCase.comment = !string.IsNullOrEmpty(request.comment) ? request.comment : existingCase.comment;
            existingCase.date = request.date ?? existingCase.date;
            existingCase.description = !string.IsNullOrEmpty(request.description) ? request.description : existingCase.description;
            existingCase.modified_by= request.modified_by ?? existingCase.modified_by;
            existingCase.modified_on=DateTime.Now;  
            // Mark the entity as modified to ensure it gets updated
            _context.Entry(existingCase).State = EntityState.Modified;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }



        [HttpDelete]
        [Route("delete_cases")]

        public async Task<IActionResult> delete_cases([FromForm] int id, [FromForm] int? deleted_by)
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
            div.deleted_by = deleted_by;    
            div.deleted_on = DateTime.Now;      

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }

        [HttpGet]
        [Route("view_cases")]
        public async Task<IActionResult> view_cases(int? id, int? category_id, int? priority_id, int? account_id, string? keyword,string? status, string? type, DateTime? fromdate, DateTime? todate, string? month,string? timeRange, int page = 1, int pageSize = 10)
        {
           // return Ok(timeRange);
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
            if (!string.IsNullOrWhiteSpace(status))
            {
                empqry = empqry.Where(e => e.status == status);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                empqry = empqry.Where(e =>
                    e.name.Contains(keyword) ||
                    e.address.Contains(keyword) ||
                     e.title.Contains(keyword) ||

                    e.location.Contains(keyword));
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

            //current day,week and month for app
            // Apply timeRange filter for current day, week, or month
            if (!string.IsNullOrEmpty(timeRange))
            {

                DateTime today = DateTime.Today;
                // return Ok(today);

                if (timeRange.Equals("day", StringComparison.OrdinalIgnoreCase))
                {
                    empqry = empqry.Where(c => c.date == today);

                }

                else if (timeRange.Equals("week", StringComparison.OrdinalIgnoreCase))
                {
                    // Start and end of the current week
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);
                    empqry = empqry.Where(c => c.date >= startOfWeek && c.date <= endOfWeek);
                }
                else if (timeRange.Equals("month", StringComparison.OrdinalIgnoreCase))
                {
                    // Start and end of the current month
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

                    empqry = empqry.Where(c => c.date >= startOfMonth && c.date <= endOfMonth);
                }
            }
            //

           
            if (!string.IsNullOrWhiteSpace(type))
            {
                empqry = empqry.Where(e => e.type == type);
            }

            // Build the query
            var query = from c in empqry
                        join s in _context.tbl_case_category on c.category_id equals s.id into cat
                        from sc in cat.DefaultIfEmpty()
                        join d in _context.tbl_case_priority on c.priority_id equals d.id into pri
                        from pr in pri.DefaultIfEmpty()
                        join p in _context.tbl_case_assembly on c.assembly_id equals p.id into ass
                        from a in ass.DefaultIfEmpty()
                        join st in _context.tbl_case_status on c.id equals st.case_id into sts
                        from sta in sts.DefaultIfEmpty()
                        where (sc == null || sc.delete_status == 0) && pr.delete_status == 0
                        group new { c, sc, pr, a, sta } by c.id into grouped // Group by c.id to remove duplicates
                        select new
                        {
                            id = grouped.Key,
                            name = grouped.First().c.name,
                            address = grouped.First().c.address,
                            email = grouped.First().c.email,
                            mobile = grouped.First().c.mobile,
                            location = grouped.First().c.location,
                            category_id = grouped.First().c.category_id,
                            category = grouped.First().sc.name, // Handle nulls from left join
                            priority_id = grouped.First().c.priority_id,
                            assembly_id = grouped.First().c.assembly_id,
                            assembly = grouped.First().a.name,
                            priority = grouped.First().pr.name,
                            activity = grouped.First().sta.remark,
                            time = grouped.First().c.time,
                            date = grouped.First().c.date,
                            title = grouped.First().c.title,
                            description = grouped.First().c.description,
                            subject = grouped.First().c.subject,
                            status = grouped.First().c.status,
                            type = grouped.First().c.type,
                            contact_person = (from ca in _context.tbl_case_contactperson
                                              where ca.case_id == grouped.Key
                                              select new
                                              {
                                                  ca.id,
                                                  contact_person = ca.name,
                                                  ca.mobile
                                              }).ToList()
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
                    result.activity,

                    result.status,
                    result.subject,

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
                  //  result.mobile,
                    result.date,
                    result.time,
                    result.assembly_id,
                    result.assembly,    
                    result.priority_id,
                    result.priority,
                    result.category_id,
                    result.category,
                    result.status,
                    result.activity,

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
                    result.category_id,
                    result.category,
                    result.date,
                    result.time,
                    result.title,
                    result.description,
                    result.activity,

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
        public async Task<IActionResult> cases_detail(int? id, string? type, int? account_id,string? timeRange)
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

            ///for app current day,month,week
            // Apply timeRange filter for current day, week, or month
            if (!string.IsNullOrEmpty(timeRange))
            {
                DateTime today = DateTime.Today;

                if (timeRange.Equals("day", StringComparison.OrdinalIgnoreCase))
                {
                    empqry = empqry.Where(c => c.date == today);
                }
                else if (timeRange.Equals("week", StringComparison.OrdinalIgnoreCase))
                {
                    // Start and end of the current week
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                    var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);
                    empqry = empqry.Where(c => c.date >= startOfWeek && c.date <= endOfWeek);
                }
                else if (timeRange.Equals("month", StringComparison.OrdinalIgnoreCase))
                {
                    // Start and end of the current month
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);
                    empqry = empqry.Where(c => c.date >= startOfMonth && c.date <= endOfMonth);
                }
            }
            ///

            var query = from c in empqry
                        join s in _context.tbl_case_category on c.category_id equals s.id into cat
                        from sc in cat.DefaultIfEmpty() // Left join with tbl_case_category
                        join d in _context.tbl_case_priority on c.priority_id equals d.id into pri
                        from pr in pri.DefaultIfEmpty() 
                        join p in _context.tbl_case_assembly on c.assembly_id equals p.id into ass
                        from a in ass.DefaultIfEmpty()
                        where
            (sc == null || sc.delete_status == 0) // Check delete_status if sc is not null
            && (pr == null || pr.delete_status == 0) // Check delete_status if pr is not null

                        select new
                        { 
                            c.id,
                            c.name,
                            c.address,
                            c.email,
                            c.mobile,
                            c.location,
                            c.category_id,
                            category = sc.name,
                            c.priority_id,
                            c.assembly_id,
                            assembly=a.name,
                            priority = pr.name,
                            c.time,
                            c.date,
                            c.title,
                            c.description,
                            c.type,
                            c.subject,
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


                          //  case status
                            case_status = (
                                from cd in _context.tbl_case_status
                                where cd.case_id == c.id
                                select new
                                {
                                    cd.id,
                                    cd.remark,
                                    cd.status,
                                    date=cd.addedon,
                                    cd.case_id,
                                    cd.name

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
           // return Ok(query);
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
                    result.assembly_id,
                    result.assembly,
                    result.subject,
                    result.status,
                    result.case_documents,
                    result.contact_person,
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
                    result.time,
                    result.priority_id,
                    result.priority,
                    result.assembly_id,
                    result.assembly,    
                    result.category_id,
                    result.category,
                    result.status,

                    result.case_documents,
                    result.contact_person,
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
                    result.category,
                    result.priority_id,
                    result.priority,
                    result.assembly_id,
                    result.assembly,
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
        public async Task<IActionResult> update_status([FromForm] int? id, [FromForm] string? status, [FromForm] string? name, [FromForm] string? remark, [FromForm] DateTime? reminder_date, [FromForm] IFormFile? document)
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
                name=name,
                file = fileName // Save the uploaded document's file name
               
            };
            await _context.tbl_case_status.AddAsync(history);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }

    }
}
