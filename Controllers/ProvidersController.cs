using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models.Enums;

namespace SmartServiceManagement.Controllers
{
    public class ProvidersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProvidersController(ApplicationDbContext context) => _context = context;
        public async Task<IActionResult> Index(string? city)
        {
            var query = _context.Providers.AsNoTracking().Include(p => p.Services).Where(p => p.Status == ProviderStatus.Approved);
            if (!string.IsNullOrWhiteSpace(city)) query = query.Where(p => p.City != null && p.City.Contains(city));
            ViewBag.City = city;
            return View(await query.OrderByDescending(p => p.AverageRating).ToListAsync());
        }
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Providers.AsNoTracking().Include(p => p.Services).ThenInclude(s => s.Category).Include(p => p.User).FirstOrDefaultAsync(p => p.ProviderId == id && p.Status == ProviderStatus.Approved);
            return item == null ? NotFound() : View(item);
        }
    }
}
