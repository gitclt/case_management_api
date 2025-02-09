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
            //validation based on field length in db
            //if (request.name.Length > 100)
            //{
            //    return BadRequest(new { status = false, message = "Name cannot exceed 100 characters." });
            //}
            //if (request.address.Length > 50)
            //{
            //    return BadRequest(new { status = false, message = "address cannot exceed 50 characters." });
            //}
            //
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
        public async Task<IActionResult> view_cases(int? id, int? category_id, int? priority_id, int? account_id, string? keyword, string? status, string? type, DateTime? fromdate, DateTime? todate, string? month, string? timeRange, int page = 1, int pageSize = 10)
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            // Base query for cases with delete_status = 0
            var empqry = _context.tbl_case_cases.Where(e => e.delete_status == 0);

            if (id.HasValue)
                empqry = empqry.Where(e => e.id == id.Value);

            if (category_id.HasValue && category_id > 0)
                empqry = empqry.Where(e => e.category_id == category_id.Value);

            if (priority_id.HasValue && priority_id > 0)
                empqry = empqry.Where(e => e.priority_id == priority_id.Value);

            if (account_id.HasValue && account_id > 0)
                empqry = empqry.Where(e => e.account_id == account_id.Value);

            if (!string.IsNullOrWhiteSpace(status))
                empqry = empqry.Where(e => e.status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                empqry = empqry.Where(e =>
                    e.name.Contains(keyword) ||
                    e.address.Contains(keyword) ||
                    e.title.Contains(keyword) ||
                    e.location.Contains(keyword));
            }

            if (fromdate.HasValue && todate.HasValue)
            {
                empqry = empqry.Where(c => c.date >= fromdate && c.date <= todate);
            }

            if (!string.IsNullOrEmpty(month))
            {
                if (int.TryParse(month, out int monthValue))
                {
                    empqry = empqry.Where(c => c.date.HasValue && c.date.Value.Month == monthValue);
                }
            }

            // Apply timeRange filter
            DateTime today = DateTime.Today;
            if (!string.IsNullOrEmpty(timeRange))
            {
                if (timeRange.Equals("day", StringComparison.OrdinalIgnoreCase))
                {
                    empqry = empqry.Where(c => c.date == today);
                }
                //else if (timeRange.Equals("week", StringComparison.OrdinalIgnoreCase))
                //{
                //    var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                //    var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);
                //    empqry = empqry.Where(c => c.date >= startOfWeek && c.date <= endOfWeek);
                //}

                if (timeRange.Equals("week", StringComparison.OrdinalIgnoreCase))
                {
                    var startOfWeek = today.Date; // Today, without time
                    var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1); // End of the 7th day
                    empqry = empqry.Where(c => c.date >= startOfWeek && c.date <= endOfWeek);
                }

                else if (timeRange.Equals("month", StringComparison.OrdinalIgnoreCase))
                {
                    var startOfMonth = new DateTime(today.Year, today.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);
                    empqry = empqry.Where(c => c.date >= startOfMonth && c.date <= endOfMonth);
                }
            }

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
                        group new { c, sc, pr, a, sta } by c.id into grouped
                        select new
                        {
                            id = grouped.Key,
                            name = grouped.First().c.name,
                            address = grouped.First().c.address,
                            email = grouped.First().c.email,
                            mobile = grouped.First().c.mobile,
                            location = grouped.First().c.location,
                            addedby = grouped.First().c.addedby,
                            account_id = grouped.First().c.account_id,

                            category_id = grouped.First().c.category_id,
                            category = grouped.First().sc.name,
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
                            comment = grouped.First().c.comment,
                            type = grouped.First().c.type,
                            contact_person = (from ca in _context.tbl_case_contactperson
                                              where ca.case_id == grouped.Key
                                              select new
                                              {
                                                  ca.id,
                                                  contact_person = ca.name,
                                                  ca.mobile,
                                                  ca.designation
                                              }).ToList()
                        };

            if (!await query.AnyAsync())
            {
                return NotFound(new { status = false, message = "No cases found" });
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var resultList = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Format response data
            var dataResponse = resultList.Select(result => new
            {
                result.id,
                result.type,
                result.name,
                result.address,
                result.email,
                result.mobile,
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
                result.comment,
                result.description,
                result.addedby,
                result.account_id,
                result.status,
                result.subject,
                result.contact_person
            }).ToList();

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

            //if (!id.HasValue) // Check if id is null
            //{
            //    return BadRequest(new { status = false, message = "ID required" });
            //}
            //if (string.IsNullOrWhiteSpace(type)) // Check if type is null or empty
            //{
            //    return BadRequest(new { status = false, message = "Type required" });
            //}

            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            // Base query for cases with delete_status = 0
            var empqry = _context.tbl_case_cases
                                 .Where(e => e.delete_status == 0);

            if (id!=null)
            {
                empqry = empqry.Where(e => e.id == id);
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
                            c.comment,

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
                                    date=cd.date,
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
            var caseList = await query.ToListAsync();


            return Ok(new
            {
                status = true,
                message = "Success.",

                data = caseList
            });
        }

        [HttpPut]
        [Route("update_status")]
        public async Task<IActionResult> update_status([FromForm] int? id, [FromForm] string? status, [FromForm] string? name, [FromForm] string? remark, [FromForm] DateTime? reminder_date, [FromForm] IFormFile? document, [FromForm] DateTime? date, [FromForm] string? type)
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

            // Update the case details
            order.status = status;
            order.reminder_date = reminder_date;
            order.date= date;       
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
                date=date,  
                type=type,
                file = fileName // Save the uploaded document's file name
               
            };
            await _context.tbl_case_status.AddAsync(history);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }

    }
}
