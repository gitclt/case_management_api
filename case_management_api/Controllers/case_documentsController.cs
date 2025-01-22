using api_case_management.Data;
using case_management_api.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace case_management_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class case_documentsController : ControllerBase
    {
        private readonly case_managementDbContext _context;

        public case_documentsController(case_managementDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        [Route("case_documentadd")]
        //public async Task<ActionResult> case_documentadd([FromForm] case_documents request, [FromForm] IFormFile? documentfile)
        //{

        //    if (documentfile != null && documentfile.Length > 0)
        //    {
        //        // Define the folder path for saving audio files
        //        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/'"+request.type+"'/"+request.case_id+"/document");
        //        if (!Directory.Exists(folderPath))
        //        {
        //            Directory.CreateDirectory(folderPath);
        //        }

        //        var fileName = Path.GetFileName(documentfile.FileName);
        //        var filePath = Path.Combine(folderPath, fileName);

        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await documentfile.CopyToAsync(stream);
        //        }
        //    }

        //    var treatment = new case_documents
        //    {
        //        account_id = request.account_id,
        //        type = request.type,
        //        document= request.document,
        //       case_id=request.case_id,
        //    };
        //    await _context.tbl_case_documents.AddAsync(treatment);
        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        status = true,
        //        message = "Data added successfully",
        //       // id = treatment.id // Access the generated ID here
        //    });
        //}

        public async Task<ActionResult> case_documentadd([FromForm] case_documents request, [FromForm] List<IFormFile> documentfiles)
        {
            if (documentfiles != null && documentfiles.Count > 0)
            {
                // Define the folder path for saving files
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/uploads/{request.type}/{request.case_id}");

                // Ensure the folder exists
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                foreach (var documentfile in documentfiles)
                {
                    if (documentfile.Length > 0)
                    {
                        // Get the file name and create the file path
                        var fileName = Path.GetFileName(documentfile.FileName);
                        var filePath = Path.Combine(folderPath, fileName);

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await documentfile.CopyToAsync(stream);
                        }

                        // Save the file details to the database
                        var treatment = new case_documents
                        {
                            account_id = request.account_id,
                            type = request.type,
                            document = fileName, // Save the file name to the database
                            case_id = request.case_id,
                        };

                        await _context.tbl_case_documents.AddAsync(treatment);
                    }
                }
                // Save changes to the database
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    status = true,
                    message = "Data added successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    status = false,
                    message = "No files were uploaded."
                });
            }
        }


    }
}
