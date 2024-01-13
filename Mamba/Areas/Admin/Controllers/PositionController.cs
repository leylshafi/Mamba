using Mamba.Areas.Admin.ViewModels;
using Mamba.Data;
using Mamba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mamba.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PositionController : Controller
    {
        private readonly AppDbContext _context;

        public PositionController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page)
        {
            double count = await _context.Positions.CountAsync();
            var positions = await _context.Positions.Skip(page*2).Take(2).Include(p=>p.Employees).ToListAsync();
            PaginationVM<Position> pagination = new()
            {
                CurrentPage = page,
                TotalPage = Math.Ceiling(count / 2),
                Items = positions
            };
            return View(pagination);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult>Create(CreatePositionVM positionVM)
        {
            if (!ModelState.IsValid) return View();
            bool result = await _context.Positions.AnyAsync(p=>p.Name== positionVM.Name);
            if(result)
            {
                ModelState.AddModelError("Name", "This position already exists");
                return View();
            }
            Position position = new Position()
            {
                Name = positionVM.Name,
            };
            await _context.Positions.AddAsync(position);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            var exsisted = await _context.Positions.FirstOrDefaultAsync(p => p.Id == id);
            if (exsisted is null) return NotFound();
            UpdatePositionVM vm = new UpdatePositionVM()
            {
                Name = exsisted.Name,
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult>Update(int id, UpdatePositionVM positionVM)
        {
            if(id<=0) return BadRequest();
            var existed = await _context.Positions.FirstOrDefaultAsync(p=>p.Id==id);
            if (existed is null) return NotFound();
            if (!ModelState.IsValid) return View(positionVM);
            bool result = await _context.Positions.AnyAsync(p => p.Name == positionVM.Name && p.Id != id);
            if (result)
            {
                ModelState.AddModelError("Name", "This position already exists");
                return View(positionVM);
            }
            existed.Name = positionVM.Name;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult>Details(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Positions.Include(p=>p.Employees).FirstOrDefaultAsync(p => p.Id == id);
            if(existed is null) return NotFound();
            return View(existed);
        }

        public async Task<IActionResult>Delete(int id)
        {
            if(id<=0) return BadRequest();
            var existed = await _context.Positions.FirstOrDefaultAsync(p => p.Id == id);
            if (existed is null) return NotFound();
            _context.Positions.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
