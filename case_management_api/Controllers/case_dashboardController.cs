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
        [HttpPost]
        [Route("case_todays_activity")]
        public async Task<ActionResult> case_todays_activity()
        {
            var curdate = DateTime.Now.Date;
           // var startOfWeek = curdate.AddDays(-7);
            //return Ok(startOfWeek);
            var supportrequest = _context.tbl_case_cases.Where(tbl => tbl.type == "support request" && tbl.delete_status == 0 && tbl.date==curdate).Count();
            var supportrequest_reminders = _context.tbl_case_cases
    .Where(tbl => tbl.type == "support request"
                  && tbl.delete_status == 0
                  && tbl.date == curdate
                  && tbl.reminder_date == curdate)
    .Count();

            var program_schedule = _context.tbl_case_cases.Where(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.date == curdate).Count();
            var programschedule_reminders = _context.tbl_case_cases
  .Where(tbl => tbl.type == "program schedule"
                && tbl.delete_status == 0
                && tbl.date == curdate
                && tbl.reminder_date == curdate)
  .Count();
            var wedding_reminder = _context.tbl_case_cases.Where(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.date == curdate).Count();
            var weddingreminders = _context.tbl_case_cases
.Where(tbl => tbl.type == "wedding reminder"
             && tbl.delete_status == 0
             && tbl.date == curdate
             && tbl.reminder_date == curdate)
.Count();

            return Ok(new
            {
                status = true,
                data = new[]
        {
            new
            {
                supportrequest = supportrequest,
                supportReminders = supportrequest_reminders,
                programschedule_reminders=programschedule_reminders,

                program_schedule = program_schedule,
                wedding_reminder =wedding_reminder ,
                weddingreminders=weddingreminders,

            }
        }
            });




        }


        [HttpGet]
        [Route("case_upcoming_activities")]
        public async Task<ActionResult> case_upcoming_activities()
        {
            var curdate = DateTime.Now.Date;
            var startOfWeek = curdate.AddDays(7);
           // return Ok(startOfWeek);
            var supportrequest = _context.tbl_case_cases.Where(tbl => tbl.type == "support request" && tbl.delete_status == 0 && tbl.reminder_date >= curdate
                  && tbl.reminder_date <= startOfWeek).Count();
            var program_schedule = _context.tbl_case_cases.Where(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.reminder_date >= curdate
                  && tbl.reminder_date <= startOfWeek).Count();
            var wedding_reminder = _context.tbl_case_cases.Where(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.reminder_date >= curdate
                  && tbl.reminder_date <= startOfWeek).Count();
            var death = _context.tbl_case_cases.Where(tbl => tbl.type == "death" && tbl.delete_status == 0 && tbl.date == startOfWeek).Count();


            return Ok(new
            {
                status = true,
                data = new[]
        {
            new
            {
                supportrequest = supportrequest,
                program_schedule = program_schedule,
                wedding_reminder =wedding_reminder ,

            }
        }
            });





        }

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
                    case "thisweek":
                        empqry = empqry.Where(c => c.date.HasValue && c.date.Value.Date >= startOfWeek && c.date.Value.Date <= endOfWeek);
                        break;
                    case "thismonth":
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





    }
}
