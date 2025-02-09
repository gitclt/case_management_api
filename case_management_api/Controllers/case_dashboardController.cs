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
        //(todays activities)
       
        [HttpGet]
        [Route("case_activities")]
        public async Task<ActionResult> case_activities(string? type, int? account_id)
        {
            var curdate = DateTime.Now.Date;
            var startOfWeek = curdate.AddDays(7);

            // ✅ Validate mandatory fields
            if (string.IsNullOrEmpty(type) || !account_id.HasValue)
            {
                return BadRequest(new
                {
                    status = false,
                    message = "type and account_id are required."
                });
            }

            // ✅ "app" type logic
            if (type == "app")
            {
                var supportrequest = _context.tbl_case_cases
                    .Count(tbl => tbl.type == "support request" &&
                                  tbl.delete_status == 0 &&
                                  tbl.date == curdate &&
                                  tbl.account_id == account_id.Value);

                var supportrequest_reminders = _context.tbl_case_cases
                    .Count(tbl => tbl.type == "support request" &&
                                  tbl.delete_status == 0 &&
                                  tbl.date == curdate &&
                                  tbl.reminder_date == curdate &&
                                  tbl.account_id == account_id.Value);

                var program_schedule = _context.tbl_case_cases
                    .Count(tbl => tbl.type == "program schedule" &&
                                  tbl.delete_status == 0 &&
                                  tbl.date == curdate &&
                                  tbl.account_id == account_id.Value);

                var programschedule_reminders = _context.tbl_case_cases
                    .Count(tbl => tbl.type == "program schedule" &&
                                  tbl.delete_status == 0 &&
                                  tbl.date == curdate &&
                                  tbl.reminder_date == curdate &&
                                  tbl.account_id == account_id.Value);
              //  return Ok(program_schedule);    
                // ✅ If all counts are 0, return 'No Data Found'
                if (supportrequest == 0 && supportrequest_reminders == 0 && program_schedule == 0 && programschedule_reminders == 0)
                {
                    return Ok(new
                    {
                        status = false,
                        message = "No data found!"
                    });
                }

                return Ok(new
                {
                    status = true,
                    data = new
                    {
                        supportrequest,
                        supportReminders = supportrequest_reminders,
                        programschedule_reminders,
                        program_schedule
                    }
                });
            }

            // ✅ "web" type logic
            else if (type == "web")
            {
                var caseData = _context.tbl_case_cases
                    .Where(tbl => tbl.delete_status == 0 &&
                                  tbl.date == curdate &&
                                  tbl.account_id == account_id)
                    .GroupBy(tbl => tbl.type)
                    .Select(g => new
                    {
                        Type = g.Key.Replace(" ", "_").ToLower(), // Convert type to snake_case format
                        Count = g.Count(),
                        ReminderCount = g.Count(tbl => tbl.reminder_date == curdate) // Count only reminders
                    })
                    .ToList();

                // ✅ If no records found, return "No data found!"
                if (!caseData.Any())
                {
                    return Ok(new
                    {
                        status = false,
                        message = "No data found!"
                    });
                }

                // Convert data dynamically into dictionary
                var responseDict = caseData.ToDictionary(
                    item => item.Type,
                    item => item.Count.ToString()
                );

                var reminderDict = caseData.ToDictionary(
                    item => $"{item.Type}_reminder",
                    item => item.ReminderCount.ToString()
                );

                // Merge both dictionaries into one
                var finalResponse = responseDict.Concat(reminderDict)
                                                .ToDictionary(k => k.Key, v => v.Value);

                return Ok(new
                {
                    status = true,
                    data = finalResponse
                });
            }

            return BadRequest(new { status = false, message = "Invalid type. Use 'app' or 'web'." });
        }

        //upcoming 
        [HttpGet]
        [Route("case_upcoming_activities")]
        public async Task<ActionResult> case_upcoming_activities(string? type, int? account_id)
        {
            var curdate = DateTime.Now.Date;
            var startOfWeek = curdate.AddDays(7);

            // Validate required parameters
            if (string.IsNullOrEmpty(type))
            {
                return BadRequest(new { status = false, message = "Type is required. Use 'app' or 'web'." });
            }

            if (!account_id.HasValue)
            {
                return BadRequest(new { status = false, message = "Account ID is required and must be an integer." });
            }
            //if (!account_id.HasValue)
            //{
            //    return BadRequest(new { status = false, message = "Invalid data type. Account ID must be an integer." });
            //}


            if (type == "app")
            {
                //var reminders = _context.tbl_case_cases
                //    .Where(tbl => tbl.delete_status == 0 &&
                //                  tbl.date > curdate &&
                //                  tbl.date <= startOfWeek &&
                //                  tbl.account_id == account_id &&
                //                  tbl.type != "program schedule" &&
                //                  tbl.type != "support request")
                //    .GroupBy(tbl => tbl.type)
                //    .Select(g => new
                //    {
                //        Label = g.Key + " Reminder", // Format label dynamically
                //        Value = g.Key.Replace(" ", "_").ToLower() + "_reminder", // Convert type to snake_case
                //        Count = g.Count().ToString() // Convert count to string
                //    })
                //    .ToList();

                var reminders = _context.tbl_case_cases
     .Where(tbl => tbl.delete_status == 0 &&
                   tbl.date >= curdate && // Ensure it includes today
                   tbl.date <= startOfWeek &&
                   tbl.account_id == account_id &&
                   tbl.type != "program schedule" &&
                   tbl.type != "support request")
     .GroupBy(tbl => tbl.type)
     .Select(g => new
     {
         Label = g.Key + " Reminder",
         Value = g.Key.Replace(" ", "_").ToLower() + "_reminder",
         Count = g.Count().ToString() // Ensure count is calculated correctly
     })
     .ToList();


                // If no reminders found, return 'No data found!'
                if (!reminders.Any())
                {
                    return Ok(new
                    {
                        status = false,
                        message = "No data found!"
                    });
                }

                return Ok(new
                {
                    status = true,
                    reminders
                });
            }

            // Web type logic
            if (type == "web")
            {
                var reminders = _context.tbl_case_cases
    .Where(tbl => tbl.delete_status == 0 &&
                  tbl.date >= curdate && // Ensure it includes today
                  tbl.date <= startOfWeek &&
                  tbl.account_id == account_id &&
                  tbl.type != "program schedule" &&
                  tbl.type != "support request")
    .GroupBy(tbl => tbl.type)
    .Select(g => new
    {
        Label = g.Key + " Reminder",
        Value = g.Key.Replace(" ", "_").ToLower() + "_reminder",
        Count = g.Count().ToString() // Ensure count is calculated correctly
    })
    .ToList();

                // If no reminders found, return 'No data found!'
                if (!reminders.Any())
                {
                    return Ok(new
                    {
                        status = false,
                        message = "No data found!"
                    });
                }

                return Ok(new
                {
                    status = true,
                    reminders
                });
            }

            return BadRequest(new { status = false, message = "Invalid type. Use 'app' or 'web'." });
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
