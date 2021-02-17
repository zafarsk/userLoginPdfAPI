using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Extensions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Wkhtmltopdf.NetCore;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly IConfiguration _config;
        readonly IGeneratePdf _generatePdf;

        public BuggyController(DataContext context, IConfiguration config, IGeneratePdf generatePdf)
        {
            _generatePdf = generatePdf;
            _context = context;
            _config = config;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }

        [HttpGet("not-found")]
        public ActionResult GetNotFound()
        {
            string fileName = HttpContext.User.GetUserName() + Path.GetRandomFileName() + ".png";
            fileName = "helloWorld.html";
            string directory = _config["StoredFilesPath"];
            string url = string.Format("{0}://{1}/{2}/{3}", HttpContext.Request.Scheme,
                                    HttpContext.Request.Host.Value, directory, fileName);
            var filePath = Path.Combine(new string[]{ Directory.GetCurrentDirectory(),
                                            directory, fileName});

            var stream = new FileStream(filePath, FileMode.Open);
            //MemoryStream pdfStream = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
            // byte[] memoryContent = pdfStream.ToArray();


            //return File(memoryContent, "application/pdf");
            return new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = "downloadedfile.pdf"
            };
            //return NotFound();
        }

        [HttpGet("export")]
        // public FileResult Export()
        public async Task<IActionResult> Export()
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                string fileName = HttpContext.User.GetUserName() + Path.GetRandomFileName() + ".png";
                fileName = "helloWorld.cshtml";
                string directory = _config["StoredFilesPath"];
                string url = string.Format("{0}://{1}/{2}/{3}", HttpContext.Request.Scheme,
                                        HttpContext.Request.Host.Value, directory, fileName);
                var filePath = Path.Combine(new string[]{ Directory.GetCurrentDirectory(),
                                            directory, fileName});
                string value = System.IO.File.ReadAllText(filePath);
                //                 StringReader reader = new StringReader(value);
                //                 Stream pdfStream = new MemoryStream(Encoding.UTF8.GetBytes(value));
                //                 Document PdfFile = new Document(PageSize.A4);

                //                 //add the collection to the document

                //                 PdfWriter writer = PdfWriter.GetInstance(PdfFile, stream);
                //                 PdfFile.Open();

                //                 XMLWorkerHelper.GetInstance().ParseXHtml(writer, PdfFile, pdfStream,Stream.Null);
                //                 PdfFile.Close();
                //                 return File(stream.ToArray(), "application/pdf", "ExportData.pdf");

                // return new ActionAsPdf("Index", new { name = "Giorgio" }) { FileName = "Test.pdf" }; 
                return await _generatePdf.GetPdfViewInHtml(value);
            }
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var appUser = _context.Users.Find(-1);
            var thingToReturn = appUser.ToString();
            return thingToReturn;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRquest()
        {
            return BadRequest("This was not a good request");
        }

    }
}