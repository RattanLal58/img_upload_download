using img_upload_download.Data;
using img_upload_download.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ImageController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public ImageController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // GET: Image/Upload
    public IActionResult Upload()
    {
        return View();
    }

    // POST: Image/Upload
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            string fileName = Path.GetFileName(file.FileName);
            string filePath = Path.Combine(_hostEnvironment.WebRootPath, "images", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save file info to database
            var image = new ImageModel
            {
                FileName = fileName,
                FilePath = Path.Combine("images", fileName)
            };

            _context.ImageModels.Add(image);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        return View();
    }

    // GET: Image/Download/5
    public IActionResult Download(int id)
    {
        var image = _context.ImageModels.FirstOrDefault(i => i.Id == id);
        if (image == null)
        {
            return NotFound();
        }

        var filePath = Path.Combine(_hostEnvironment.WebRootPath, image.FilePath);
        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/octet-stream", image.FileName);
    }

    // GET: Image/Index
    public IActionResult Index()
    {
        var images = _context.ImageModels.ToList();
        return View(images);
    }
}
