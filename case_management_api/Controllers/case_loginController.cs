using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_loginController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_loginController(case_managementDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("generate_tocken")]
        public async Task<string> generate_tocken()
        {
            // Generate a new GUID
            Guid enc_key = Guid.NewGuid();

            // Format the GUID and add the prefix
            string tokenString = "tocken" + enc_key.ToString().Replace("-", "").Substring(0, 10);

            // Check if the generated token exists in the database
            bool exists = await _context.tbl_case_login.AnyAsync(u => u.enc_key == tokenString);

            // If a duplicate is found, recursively generate a new token
            if (exists)
            {
                return await generate_tocken();
            }

            // Return the unique token
            return tokenString;
        }

        [HttpPost]

        [Route("Login")]
        public async Task<IActionResult> Login([FromForm] string? data)
        {
            try
            {
                // Step 1: Check if encrypted data is provided
                if (string.IsNullOrEmpty(data))
                {
                    return BadRequest(new { status = false, message = "No data provided" });
                }

                // Step 2: Decrypt the encrypted data
                string decryptedData = _context.base64Decode(data);
                // Step 3: Split the decrypted data to extract username, password, and type
                var parameters = decryptedData.Split('&');
                string username = parameters.FirstOrDefault(p => p.StartsWith("username="))?.Split('=')[1];
                string password = parameters.FirstOrDefault(p => p.StartsWith("password="))?.Split('=')[1];
                string type = parameters.FirstOrDefault(p => p.StartsWith("type="))?.Split('=')[1];
                //  return Ok(new { username, password, type });        


                var users = await _context.tbl_case_login
           .Where(tbl => tbl.username == username && tbl.password == password && tbl.delete_status == 0)
           .ToListAsync();

                if (users.Count == 0)
                {
                    return Ok(new { status = false, message = "invalid credentials " });

                }


                // return Ok(users);
                var token = data = users.First().enc_key;


                if (token == null)
                {
                    foreach (var user in users)
                    {
                        if (string.IsNullOrEmpty(user.enc_key))
                        {
                            // Generate a new token if none exists
                            user.enc_key = await generate_tocken();
                            user.enc_key_date = DateTime.UtcNow;
                        }
                        else
                        {
                            // Check if the token is outdated and regenerate if necessary
                            DateTime currentDate = DateTime.UtcNow.Date;
                            if (user.enc_key_date?.Date != currentDate)
                            {
                                user.enc_key = await generate_tocken();
                                user.enc_key_date = currentDate;
                            }
                        }
                    }

                    await _context.SaveChangesAsync(); // Save changes to update tokens in the database

                }

                // Step 4: Check if both username, password, and type are present
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(type))
                {
                    return BadRequest(new { status = false, message = "Username, password, or type not provided" });
                }

                // Step 5: Handle login based on type (employee or customer)
                if (type == "admin")
                {
                    return Ok(new { status = true, message = "success",
                        data = 
    
        new
        {
            enc_key = users.First().enc_key,
            account_id = users.First().account_id
        }
    
                    });
                }


                else if (type == "subadmin")
                {
                    return Ok(new { status = true, message = "success", data = users.First().enc_key, users.First().account_id });
                }
                else
                {
                    return BadRequest(new { status = false, message = "Invalid type provided" });
                }
            }
            catch (Exception ex)
            {
                // Handle any unexpected exceptions
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]

        [Route("sign_in")]
        public async Task<IActionResult> sign_in([FromForm] string? mobile)
        {
            string otp = "";
            var user = await _context.tbl_case_login
                .Where(u => u.mobile == mobile && u.delete_status == 0)
                .FirstOrDefaultAsync();

            var random = new Random();
            otp = random.Next(1000, 9999).ToString(); 

            if (user != null)
            {
                user.otp = Convert.ToInt32(otp);

                await _context.SaveChangesAsync();

                return Ok(new { status = true, message = "Successfully logged in", data = new { otp = user.otp } });
            }

            return Ok(new { status = false, message = "Please do register using this number" });
        }



        [HttpPost]
        [Route("otp_verification")]
        public async Task<IActionResult> otp_verification([FromForm] int? otp, [FromForm] string? mobile)
        {

            if (otp == null)
            {
                return Ok(new { status = false, message = "No data provided" });
            }

            var user = await _context.tbl_case_login
            .Where(u => u.otp == otp && u.delete_status == 0 && u.mobile == mobile)
            .FirstOrDefaultAsync();


            if (user != null)
            {
                if (string.IsNullOrEmpty(user.enc_key))
                {
                    user.enc_key = await generate_tocken();
                    user.enc_key_date = DateTime.UtcNow;
                }
                else
                {
                    DateTime currentDate = DateTime.UtcNow.Date;
                    if (user.enc_key_date?.Date != currentDate)
                    {
                        user.enc_key = await generate_tocken();
                        user.enc_key_date = currentDate;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { status = true, message = "Successfully logged in", data = new { id = user.id, token = user.enc_key,account_id=user.account_id } });
            }

            return Ok(new { status = false, message = "Invalid credentials" });


        }

        [HttpGet]

        [Route("Profile")]
        //public async Task<IActionResult> Profile(string encKey)
        //{
        //    if (string.IsNullOrEmpty(encKey))
        //    {
        //        return BadRequest(new { status = false, message = "Token is missing." });
        //    }

        //    var baseUrl = $"{Request.Scheme}://{Request.Host}/";

        //    // Query to fetch employee profile along with department details
        //    var details = await (
        //        from a in _context.tbl_case_login
        //        join ca in _context.tbl_case_account on a.account_id equals ca.id
        //        where a.enc_key == encKey
        //        select new
        //        {
        //            a.id,
        //            a.username,
        //            a.account_id,
        //            a.name,
        //            a.mobile,
        //            account = ca.name,
        //            ca.email,
        //            ca.description
        //        })
        //        .ToListAsync();

        //    // Check if employee exists
        //    if (details == null || !details.Any())  // Check for an empty list
        //    {
        //        return NotFound(new { status = false, message = "No profile found for the provided token." });
        //    }

        //    // Return employee profile data if found (assuming we return the first profile if there are multiple results)
        //    var employee = details.FirstOrDefault();

        //    return Ok(new
        //    {
        //        status = true,
        //        message = "Success.",
        //        data = details 
        //    });
        //}

        public async Task<IActionResult> Profile(string encKey)
        {
            if (string.IsNullOrEmpty(encKey))
            {
                return BadRequest(new { status = false, message = "Token is missing." });
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}/";

            // Query to fetch employee profile along with department details
            var employee = await (
                from a in _context.tbl_case_login
                join ca in _context.tbl_case_account on a.account_id equals ca.id
                where a.enc_key == encKey
                select new
                {
                    a.id,
                    a.username,
                    a.account_id,
                    a.name,
                    a.mobile,
                    account = ca.name,
                    ca.email,
                    ca.description
                })
                .FirstOrDefaultAsync(); 

            // Check if employee exists
            if (employee == null)
            {
                return NotFound(new { status = false, message = "No profile found for the provided token." });
            }

            // Return employee profile data if found
            return Ok(new
            {
                status = true,
                message = "Success.",
                data = employee // No list, only a single object
            });
        }


        [HttpPut]
        [Route("update_fcm")]
        public async Task<IActionResult> update_fcm([FromForm] int? id, [FromForm] string? fcm)
        {
            if (id == null)
            {
                return BadRequest(new { status = false, message = "ID is required" });
            }

            var order = await _context.tbl_case_login.FindAsync(id);

            if (order == null)
            {
                return NotFound(new { status = false, message = "Record not found" });
            }

            // Update FCM token if provided
            if (!string.IsNullOrEmpty(fcm))
            {
                order.fcm = fcm;
            }

            _context.tbl_case_login.Update(order);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Data updated successfully" });
        }




    }
}
