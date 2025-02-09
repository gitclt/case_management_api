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


        [HttpDelete]
        [Route("case_document_delete")]
        public async Task<ActionResult> case_document_delete([FromForm] int id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/";

            // Find the document in the database
            var document = await _context.tbl_case_documents.FindAsync(id);

            if (document == null)
            {
                return NotFound(new { status = false, message = "Document not found" });
            }

            // Get the file path
            string documentPath = !string.IsNullOrEmpty(document.document)
                      ? $"{baseUrl}uploads/{document.type}/{document.id}/{document.document}"
                      : null;


            // Delete the file from the server
            if (!string.IsNullOrEmpty(documentPath) && System.IO.File.Exists(documentPath))
            {
                System.IO.File.Delete(documentPath);
            }

            // Remove the document entry from the database
            _context.tbl_case_documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Document deleted successfully" });
        }


        [HttpGet]
        [Route("view_documents")]

        public async Task<IActionResult> view_documents(int? case_id, int? account_id)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}/";

            if (_context.tbl_case_documents == null)
            {
                return Problem("Entity set '_context.tbl_case_documents' is null.");
            }

            var documents = (
                from cd in _context.tbl_case_documents
                join c in _context.tbl_case_cases on cd.case_id equals c.id
                where (!case_id.HasValue || cd.case_id == case_id && !account_id.HasValue || c.account_id == account_id) // Filter by case_id if provided
                select new
                {
                    cd.id,
                    document_path = !string.IsNullOrEmpty(cd.document)
                        ? $"{baseUrl}uploads/{c.type}/{c.id}/{cd.document}"
                        : null
                }
            ).ToList(); // Convert to list

            return Ok(new
            {
                status = true,
                Message = "Success.",
                data = documents // Corrected variable name
            });
        }


    }
}
