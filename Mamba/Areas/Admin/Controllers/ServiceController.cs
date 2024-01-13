using Mamba.Areas.Admin.ViewModels;
using Mamba.Data;
using Mamba.Models;
using Mamba.Utilities.Extentions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mamba.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ServiceController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Services.CountAsync();
            var services = await _context.Services.Skip(page*2).Take(2).ToListAsync();
            PaginationVM<Service> paginationVM = new()
            {
                CurrentPage= page,
                TotalPage = Math.Ceiling(count/2),
                Items = services
            };
            return View(paginationVM);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceVM serviceVM)
        {
            if (!ModelState.IsValid) return View();
            if (!serviceVM.Photo.ValidateType())
            {
                ModelState.AddModelError("Photo", "Invalid type");
            }
            if (!serviceVM.Photo.ValidateSize())
            {
                ModelState.AddModelError("Photo", "Invalid size");
            }
            bool result = await _context.Services.AnyAsync(s=>s.Name==serviceVM.Name);  
            if(result)
            {
                ModelState.AddModelError("Name", "This service already exists");
                return View();
            }
            Service service = new Service()
            {
                Name = serviceVM.Name,
                Description = serviceVM.Description,
                ImageUrl = await serviceVM.Photo.CreateFile(_env.WebRootPath, "assets", "img")
            };
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult>Update(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            UpdateServiceVM vm = new()
            {
                Name = existed.Name,
                Description = existed.Description,
                ImageUrl = existed.ImageUrl,
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult>Update(int id, UpdateServiceVM serviceVM)
        {
            if(id <= 0) return BadRequest();
            var existed = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            if(serviceVM.Photo is not null)
            {
                if (!serviceVM.Photo.ValidateType())
                {
                    ModelState.AddModelError("Photo", "Invalid type");
                }
                if (!serviceVM.Photo.ValidateSize())
                {
                    ModelState.AddModelError("Photo", "Invalid size");
                }
                await existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "img");
                existed.ImageUrl = await serviceVM.Photo.CreateFile(_env.WebRootPath,"assets","img");
            }

            existed.Name = serviceVM.Name;
            existed.Description = serviceVM.Description;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            if(id<=0) return BadRequest();
            var existed= await _context.Services.FirstOrDefaultAsync(s=>s.Id== id); 
            if (existed is null) return NotFound();
            return View(existed);
        }

        public async Task<IActionResult>Delete(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            await existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "img");
            _context.Services.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
