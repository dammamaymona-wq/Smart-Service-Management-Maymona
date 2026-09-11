using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models.Enums;

namespace SmartServiceManagement.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ServicesController(ApplicationDbContext context) => _context = context;
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _context.Services.AsNoTracking().Include(s => s.Category).Include(s => s.Provider).Where(s => s.IsActive && s.Provider.Status == ProviderStatus.Approved);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(s => s.ServiceName.Contains(search) || s.Description.Contains(search));
            if (categoryId.HasValue) query = query.Where(s => s.CategoryId == categoryId.Value);
            ViewBag.Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Search = search; ViewBag.CategoryId = categoryId;
            return View(await query.OrderByDescending(s => s.CreatedAt).ToListAsync());
        }
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Services.AsNoTracking().Include(s => s.Category).Include(s => s.Provider).ThenInclude(p => p.User).FirstOrDefaultAsync(s => s.ServiceId == id && s.IsActive);
            return item == null ? NotFound() : View(item);
        }
    }
}
