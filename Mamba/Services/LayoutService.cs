using Mamba.Data;
using Mamba.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Mamba.Services
{
    public class LayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LayoutVM> GetSettings()
        {
            var setting = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            var vm = new LayoutVM()
            {
                Setting = setting
            };
            return vm;
        }
    }
}
