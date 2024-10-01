using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using PDF_AI.Models;
using System.Diagnostics;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;
using PdfSharp;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using ImageMagick;

namespace PDF_AI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string fileName)
        {
            string fullFileName = fileName + ".pdf";

            var tempPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "temp", fileName);

            //var tempDirectory = Path.GetDirectoryName(tempPath);
            //if (!Directory.Exists(tempDirectory))
            //{
            //    Directory.CreateDirectory(tempDirectory);
            //}

            //using (var stream = new FileStream(tempPath, FileMode.Create))
            //{
            //    file.CopyTo(stream);
            //}

            //using (PdfDocument originalDocument = PdfReader.Open(tempPath, PdfDocumentOpenMode.Import))
            //{
                using (PdfDocument newDocument = new PdfDocument())
                {                   

                    XGraphics gfx = XGraphics.FromPdfPage(newDocument.Pages[0]);
                    XFont font = new XFont("Verdana", 20, XFontStyleEx.Bold);
                    gfx.DrawString("Hello, World!", font, XBrushes.Black, new XRect(50, 50, newDocument.Pages[0].Width, newDocument.Pages[0].Height), XStringFormats.TopLeft);

                    // Example: Drawing a rectangle on the page
                    gfx.DrawRectangle(XBrushes.LightBlue, 100, 100, 200, 100);

                    newDocument.Save(tempPath);
                };               
            //};

                var document = new FileData
                {
                    File = "/temp/" + fullFileName,
                };

            return View(document);
        }
        
        [HttpPost]
        public IActionResult GetText(IFormFile file)
        {
            var tempPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "temp", file.FileName);

            var tempDirectory = Path.GetDirectoryName(tempPath);
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }


            // Magick.net convert pdf to image
            // Settings the density to 300 dpi will create an image with a better quality
            var settings = new MagickReadSettings
            {
                Density = new Density(300)
            };

            using var images = new MagickImageCollection();

            // Add all the pages of the pdf file to the collection
            images.Read(tempPath, settings);

            // Create new image that appends all the pages horizontally
            using var horizontal = images.AppendHorizontally();

            var newTempPath = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "temp", Path.GetFileNameWithoutExtension(file.FileName) + ".png");

            // Save result as a png
            horizontal.Write(Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "temp", newTempPath));

            // Create new image that appends all the pages vertically
            using var vertical = images.AppendVertically();

            // Save result as a png
            vertical.Write(Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "temp", newTempPath));



            var tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            

            using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(newTempPath))
                {
                    using (var page = engine.Process(img))
                    {
                        var text = page.GetText();

                        var textData = new FileData
                        {
                            File = text,
                            FileLocation = "/temp/" + Path.GetFileNameWithoutExtension(file.FileName) + ".png",
                        };

                        return View("Index", textData);
                    }
                }
            }               
        }
        

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
