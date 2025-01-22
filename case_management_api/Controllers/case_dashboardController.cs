using api_case_management.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            var startOfWeek = curdate.AddDays(-7);
            //return Ok(startOfWeek);
            var supportrequest = _context.tbl_case_cases.Where(tbl => tbl.type == "support request" && tbl.delete_status == 0 && tbl.date==curdate).Count();
            var program_schedule = _context.tbl_case_cases.Where(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.date == curdate).Count();
            var wedding_reminder = _context.tbl_case_cases.Where(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.date == curdate).Count();
             

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


        [HttpPost]
        [Route("case_upcoming_activities")]
        public async Task<ActionResult> case_upcoming_activities()
        {
            var curdate = DateTime.Now.Date;
            var startOfWeek = curdate.AddDays(+7);
           // return Ok(startOfWeek);
            var supportrequest = _context.tbl_case_cases.Where(tbl => tbl.type == "support request" && tbl.delete_status == 0 && tbl.date == startOfWeek).Count();
            var program_schedule = _context.tbl_case_cases.Where(tbl => tbl.type == "program schedule" && tbl.delete_status == 0 && tbl.date == startOfWeek).Count();
            var wedding_reminder = _context.tbl_case_cases.Where(tbl => tbl.type == "wedding reminder" && tbl.delete_status == 0 && tbl.date == startOfWeek).Count();
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






    }
}
