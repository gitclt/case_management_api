using System.Data;
using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_roleprivilageController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_roleprivilageController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost("role_privilage_add")]
        public async Task<IActionResult> role_privilage_add([FromBody] List<case_role_privilage> jsonData)
        {
            try
            {
                foreach (var user_privilege in jsonData)
                {
                    var userRolePrivilage = new case_role_privilage
                    {
                        privilage_id = user_privilege.privilage_id,
                        is_add = user_privilege.is_add,
                        is_delete = user_privilege.is_delete,
                        is_view = user_privilege.is_view,
                        is_edit = user_privilege.is_edit,
                        role_id = user_privilege.role_id,
                        account_id = user_privilege.account_id,
                    };

                    _context.tbl_case_role_privilage.Add(userRolePrivilage);
                }

                await _context.SaveChangesAsync();

                return Ok(new { status = true, message = "Added Successfully" });
            }
            catch (Exception ex)
            {
                // Handle and log the exception if necessary
                return StatusCode(500, new { status = false, message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost("user_role_privilage_update")]
        public async Task<IActionResult> user_role_privilage_update([FromBody] List<case_role_privilage> jsonData)
        {
            if (jsonData == null || !jsonData.Any())
            {
                return BadRequest(new { status = false, message = "Invalid data" });
            }

            foreach (var user_privillage in jsonData)
            {
                var data = await _context.tbl_case_role_privilage.FindAsync(user_privillage.id);
                if (data == null)
                {
                    return NotFound(new { status = false, message = $" role privilege with ID {user_privillage.id} not found" });
                }


                if (user_privillage.privilage_id != null)
                {
                    data.privilage_id = user_privillage.privilage_id;
                }
                if (user_privillage.is_add != null)
                {
                    data.is_add = user_privillage.is_add;
                }
                if (user_privillage.is_delete != null)
                {
                    data.is_delete = user_privillage.is_delete;
                }
                if (user_privillage.is_view != null)
                {
                    data.is_view = user_privillage.is_view;
                }
                if (user_privillage.is_edit != null)
                {
                    data.is_edit = user_privillage.is_edit;
                }
                if (user_privillage.role_id != null)
                {
                    data.role_id = user_privillage.role_id;
                }
            }


            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Updated Successfully" });
        }

        [HttpGet("role_privilage_list")]
        public IActionResult role_privilage_list(int? role_id)
        {
            var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Specify the path to your appsettings.json
            .AddJsonFile("appsettings.json"); // Load the appsettings.json file

            // Build the configuration
            var configuration = configBuilder.Build();
            string connectionString = configuration.GetConnectionString("adminconnection1");
            //team full under and upper all 

            //            string q = @"SELECT id,name,state_id,branch_code
            //FROM tbl_branch where id in ("+ result.BranchIds.TrimStart(',').TrimEnd(',') + ")";

            string role_privilage = @"select p.module,p.menu,p.platform,up.is_add,up.is_delete,up.is_view,up.is_edit,up.privilage_id,up.id,r.id as role_id from tbl_case_role r
        join tbl_case_role_privilage up on r.id =up.role_id
        join tbl_case_privilage p on p.id=up.privilage_id
        where r.id=" + role_id + " ";
            //return Ok(role_privilage);
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {

                var privilage = dbConnection.Query<case_management_api.Model.case_role_privilage>(role_privilage);

                return Ok(new { status = true, message = "success", privilage = privilage });

            }

        }

        [HttpDelete]
        [Route("delete_role_privilage")]
        public async Task<IActionResult> delete_role_privilage(int id)
        {

            if (_context.tbl_case_role_privilage == null)
            {
                return Problem("Entity set '_context.tbl_case_role_privilage' is null.");
            }

            var div = await _context.tbl_case_role_privilage.FindAsync(id);
            // return Ok(div);

            if (div == null)
            {
                return NotFound(new { status = false, message = "not found" });
            }
            _context.tbl_case_role_privilage.Remove(div);

            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data deleted successfully" });
        }


    }
}
