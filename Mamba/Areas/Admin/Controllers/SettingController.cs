using Mamba.Areas.Admin.ViewModels;
using Mamba.Data;
using Mamba.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Mamba.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SettingController : Controller
    {
        private readonly AppDbContext _context;

        public SettingController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.ToListAsync();
            return View(settings);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult>Create(CreateSettingVM settingVM)
        {
            if (!ModelState.IsValid) return View();
            bool result = await _context.Settings.AnyAsync(s=>s.Key==settingVM.Key);
            if(result)
            {
                ModelState.AddModelError("Key", "There is already such key");
                return View();
            }
            Setting setting = new()
            {
                Key = settingVM.Key,
                Value = settingVM.Value
            };
            await _context.Settings.AddAsync(setting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            UpdateSettingVM vm = new()
            {
                Key = existed.Key,
                Value = existed.Value
            };
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int id, CreateSettingVM settingVM)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            bool result = await _context.Settings.AnyAsync(s => s.Key == settingVM.Key && s.Id!=id);
            if (result)
            {
                ModelState.AddModelError("Key", "There is already such key");
                return View();
            }
            if (!ModelState.IsValid) return View(settingVM);
            existed.Key = settingVM.Key;
            existed.Value = settingVM.Value;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult>Delete(int id)
        {
            if (id <= 0) return BadRequest();
            var existed = await _context.Settings.FirstOrDefaultAsync(s => s.Id == id);
            if (existed is null) return NotFound();
            _context.Settings.Remove(existed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
