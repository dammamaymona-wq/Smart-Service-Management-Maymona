using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;

namespace SmartServiceManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ServiceRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { _context = context; _userManager = userManager; }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var items = await _context.ServiceRequests.Where(r => r.CustomerId == userId).Include(r => r.Service).ThenInclude(s => s.Provider).OrderByDescending(r => r.RequestDate).ToListAsync();
            return View(items);
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var item = await _context.ServiceRequests.Where(r => r.CustomerId == userId).Include(r => r.Service).ThenInclude(s => s.Provider).FirstOrDefaultAsync(r => r.RequestId == id);
            return item == null ? NotFound() : View(item);
        }
    }
}
