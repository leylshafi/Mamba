using Mamba.Areas.Admin.ViewModels;
using Mamba.Data;
using Mamba.Models;
using Mamba.Utilities.Extentions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mamba.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Employees.CountAsync();
            var employees = await _context.Employees.Skip(page * 2).Take(2).Include(e => e.Position).ToListAsync();
            PaginationVM<Employee> paginationVM = new()
            {
                CurrentPage = page,
                TotalPage = Math.Ceiling(count / 2),
                Items = employees,
            };
            return View(paginationVM);
        }

        public async Task<IActionResult> Create()
        {
            CreateEmployeeVM vm = new()
            {
                Positions = await _context.Positions.ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeVM employeeVm)
        {
            if (!ModelState.IsValid)
            {
                employeeVm.Positions = await _context.Positions.ToListAsync();
                return View(employeeVm);
            }
            if (!employeeVm.Photo.ValidateType())
            {
                ModelState.AddModelError("Photo", "Invalid type");
                employeeVm.Positions = await _context.Positions.ToListAsync();
                return View(employeeVm);
            }
            if (!employeeVm.Photo.ValidateSize())
            {
                ModelState.AddModelError("Photo", "Invalid size");
                employeeVm.Positions = await _context.Positions.ToListAsync();
                return View(employeeVm);
            }

            bool result = await _context.Employees.AnyAsync(e => e.Name == employeeVm.Name && e.Surname == employeeVm.Surname);
            if (result)
            {
                ModelState.AddModelError("Name", "This employee already exists");
                employeeVm.Positions = await _context.Positions.ToListAsync();
                return View(employeeVm);
            }

            Employee employee = new()
            {
                Name = employeeVm.Name,
                Surname = employeeVm.Surname,
                PositionId = employeeVm.PositionId,
                TwitterLink = employeeVm.TwitterLink,
                FacebookLink = employeeVm.FacebookLink,
                InstagramLink = employeeVm.InstagramLink,
                LinkedinLink = employeeVm.LinkedinLink,
                ImageUrl = await employeeVm.Photo.CreateFile(_env.WebRootPath, "assets", "img")
            };
            await _context.Employees.AddAsync(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();
            UpdateEmployeeVM employeeVM = new UpdateEmployeeVM()
            {
                Name = existed.Name,
                Surname = existed.Surname,
                FacebookLink = existed.FacebookLink,
                InstagramLink = existed.InstagramLink,
                LinkedinLink = existed.LinkedinLink,
                TwitterLink = existed.TwitterLink,
                PositionId = existed.PositionId,
                ImageUrl = existed.ImageUrl,
                Positions = await _context.Positions.ToListAsync()
            };

            return View(employeeVM);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, UpdateEmployeeVM employeeVM)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();
            if (!ModelState.IsValid)
            {
                employeeVM.ImageUrl = existed.ImageUrl;
                employeeVM.Positions = await _context.Positions.ToListAsync();
                return View(employeeVM);
            }
            if (employeeVM.Photo is not null)
            {
                if (!employeeVM.Photo.ValidateType())
                {
                    ModelState.AddModelError("Photo", "Invalid type");
                    employeeVM.Positions = await _context.Positions.ToListAsync();
                    employeeVM.ImageUrl = existed.ImageUrl;
                    return View(employeeVM);
                }
                if (!employeeVM.Photo.ValidateSize())
                {
                    ModelState.AddModelError("Photo", "Invalid size");
                    employeeVM.Positions = await _context.Positions.ToListAsync();
                    employeeVM.ImageUrl = existed.ImageUrl;
                    return View(employeeVM);
                }
                await existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "img");
                existed.ImageUrl = await employeeVM.Photo.CreateFile(_env.WebRootPath, "assets", "img");
            }

            existed.Name = employeeVM.Name;
            existed.Surname = employeeVM.Surname;
            existed.FacebookLink = employeeVM.FacebookLink;
            existed.TwitterLink = employeeVM.TwitterLink;
            existed.InstagramLink = employeeVM.InstagramLink;
            existed.LinkedinLink = employeeVM.LinkedinLink;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult>Details(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Employees.Include(e=>e.Position).FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();
            return View(existed);
        }
        public async Task<IActionResult>Delete(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Employees.Include(e => e.Position).FirstOrDefaultAsync(e => e.Id == id);
            if (existed is null) return NotFound();
            await existed.ImageUrl.DeleteFile(_env.WebRootPath, "assets", "img");
            _context.Employees.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
