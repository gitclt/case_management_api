using api_case_management.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_dashboardController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_dashboardController(case_managementDbContext context)
        {
            _context = context;
        }
        //(todays and upcoming activities)
        [HttpGet]
        [Route("case_activities")]
        public async Task<ActionResult> case_activities(string type)
        {
            var curdate = DateTime.Now.Date;
            var startOfWeek = curdate.AddDays(7);

            if (type == "today")
            {
                var supportrequest = _context.tbl_case_cases.Count(tbl => tbl.type == "support request" && tbl.delete_status == 0 && tbl.date == curdate);
                var supportrequest_reminders = _context.tbl_case_cases.Count(tbl => tbl.type == "support request" && tbl.delete_status == 0 && tbl.date == curdate && tbl.reminder_date == curdate);

                var program_schedule = _context.tbl_case_cases.Count(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.date == curdate);
                var programschedule_reminders = _context.tbl_case_cases.Count(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.date == curdate && tbl.reminder_date == curdate);

                var wedding_reminder = _context.tbl_case_cases.Count(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.date == curdate);
                var weddingreminders = _context.tbl_case_cases.Count(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.date == curdate && tbl.reminder_date == curdate);

                return Ok(new
                {
                    status = true,
                    data = new[]
                    {
                new
                {
                    supportrequest,
                    supportReminders = supportrequest_reminders,
                    programschedule_reminders,
                    program_schedule,
                    wedding_reminder,
                    weddingreminders
                }
            }
                });
            }
            else if (type == "upcoming")
            {
               // var supportrequest = _context.tbl_case_cases.Count(tbl => tbl.type == "support request" && tbl.delete_status == 0 && tbl.date == startOfWeek);

                var program_schedule = _context.tbl_case_cases.Count(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.date >= curdate && tbl.date <= startOfWeek);
                var program_schedule_reminders = _context.tbl_case_cases.Count(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.reminder_date >= curdate && tbl.reminder_date <= startOfWeek);
                var wedding = _context.tbl_case_cases.Count(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.date >= curdate && tbl.date <= startOfWeek);

                var wedding_reminder = _context.tbl_case_cases.Count(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.reminder_date >= curdate && tbl.reminder_date <= startOfWeek);

                return Ok(new
                {
                    status = true,
                    data = new[]
                    {
                new
                {
                    program_schedule,
                    program_schedule_reminders,
                    wedding,
                    wedding_reminder
                }
            }
                });
            }
            else
            {
                return BadRequest(new { status = false, message = "Invalid type. Use 'today' or 'upcoming'." });
            }
        }

        //
        [HttpGet]
        [Route("view_upcoming_activities")]
        public async Task<IActionResult> view_upcoming_activities(int? id, string? type, string? period, int page = 1, int pageSize = 10)
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            var curdate = DateTime.Now.Date;
            var startOfWeek = curdate.AddDays(-(int)curdate.DayOfWeek); // Start of the current week (Sunday)
            var endOfWeek = startOfWeek.AddDays(6); // End of the current week (Saturday)
            var startOfMonth = new DateTime(curdate.Year, curdate.Month, 1); // Start of the current month
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // End of the current month

            // Base query for cases with delete_status = 0
            var empqry = _context.tbl_case_cases
                                 .Where(e => e.delete_status == 0);

            if (id.HasValue)
            {
                empqry = empqry.Where(e => e.id == id.Value);
            }

            // Apply filters for this day, this week, or this month based on the `period` parameter
            if (!string.IsNullOrWhiteSpace(period))
            {
                switch (period.ToLower())
                {
                    case "today":
                        empqry = empqry.Where(c => c.date.HasValue && c.date.Value.Date == curdate);
                        break;
                    case "week":
                        empqry = empqry.Where(c => c.date.HasValue && c.date.Value.Date >= startOfWeek && c.date.Value.Date <= endOfWeek);
                        break;
                    case "month":
                        empqry = empqry.Where(c => c.date.HasValue && c.date.Value.Date >= startOfMonth && c.date.Value.Date <= endOfMonth);
                        break;
                    default:
                        break;
                }
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
                            ).ToList()
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
                    result.location,
                    result.title,
                    result.description,
                    result.mobile,
                    result.date,
                    result.time,
                    result.priority_id,
                    result.priority,
                    result.category_id,
                    result.contact_person,
                    result.status,
                }).ToList();
            }
            else
            {
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

        //app
        [HttpGet]
        [Route("total_cases")]
        public async Task<ActionResult> total_cases()
        {
            try
            {
                // Get the current date
                var curdate = DateTime.Now.Date;

                // Fetch the count of total cases for the current date
                var total_cases = await _context.tbl_case_cases
                    .Where(tbl => tbl.delete_status == 0 && tbl.date.Value == curdate)
                    .CountAsync();

                // Return the response
                return Ok(new
                {
                    status = true,
                    data = new
                    {
                        total_cases
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors gracefully
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while fetching total cases.",
                    error = ex.Message
                });
            }
        }

        //todays reminders

        [HttpGet]
        [Route("todays_reminders")]
        public async Task<IActionResult> todays_reminders()
        {
            if (_context.tbl_case_cases == null)
            {
                return Problem("Entity set '_context.tbl_case_cases' is null.");
            }

            var curdate = DateTime.Now.Date;

            // Base query for cases with delete_status = 0 and today's reminder date
            var empqry = _context.tbl_case_cases
                                 .Where(e => e.delete_status == 0 && e.reminder_date == curdate);

            // Build the query
            var query = from c in empqry
                        join s in _context.tbl_case_category on c.category_id equals s.id into cat
                        from sc in cat.DefaultIfEmpty() 
                        join d in _context.tbl_case_priority on c.priority_id equals d.id into pri
                        from pr in pri.DefaultIfEmpty() 
                        join p in _context.tbl_case_assembly on c.assembly_id equals p.id into ass
                        from aa in ass.DefaultIfEmpty() 
                        where (sc == null || sc.delete_status == 0) 
          && (pr == null || pr.delete_status == 0) && (aa == null || aa.delete_status == 0)
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
                            assembly = aa.name,
                            priority = pr.name,
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
                                    contact_person = ca.name,
                                    ca.mobile
                                }
                            ).ToList()
                        };

            // Check if the query has any results
            if (!await query.AnyAsync())
            {
                return NotFound(new { status = false, message = "No cases found" });
            }

            // Retrieve the results
            var resultList = await query.ToListAsync();

            // Format the response
            var dataResponse = resultList.Select(result => new
            {
                result.id,
                result.location,
                result.title,
                result.description,
                result.mobile,
                result.date,
                result.time,
                result.priority_id,
                result.priority,
                result.category_id,
                result.status,

                result.contact_person,
            }).ToList();

            return Ok(new
            {
                status = true,
                message = "Success.",
                data = dataResponse
            });
        }

        [HttpGet]
        [Route("cases_count")]
        public async Task<ActionResult> cases_count()
        {
            try
            {
                // Get the current date
                var curdate = DateTime.Now.Date;

                // Fetch the count of total cases for the current date
                //var todays_cases = await _context.tbl_case_cases
                //    .Where(tbl => tbl.delete_status == 0 && tbl.date.Value == curdate)
                //    .CountAsync();

                var total_cases = await _context.tbl_case_cases
                   .Where(tbl => tbl.delete_status == 0)
                   .CountAsync();

                //var pending_cases = await _context.tbl_case_cases
                //    .Where(tbl => tbl.delete_status == 0 && tbl.status == "pending")
                //    .CountAsync();

                //var completed_cases = await _context.tbl_case_cases
                //   .Where(tbl => tbl.delete_status == 0 && tbl.status == "completed")
                //   .CountAsync();

                // Return the response
                return Ok(new
                {
                    status = true,
                    data = new
                    {
                        total_cases
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors gracefully
                return StatusCode(500, new
                {
                    status = false,
                    message = "An error occurred while fetching total cases.",
                    error = ex.Message
                });
            }
        }










    }
}
