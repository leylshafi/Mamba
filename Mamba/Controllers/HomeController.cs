using Mamba.Data;
using Mamba.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mamba.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            HomeVM vm = new HomeVM()
            {
                Services = await _context.Services.ToListAsync(),
                Employees = await _context.Employees.Include(e=>e.Position).ToListAsync(),
            };
            return View(vm);
        }

       
    }
}