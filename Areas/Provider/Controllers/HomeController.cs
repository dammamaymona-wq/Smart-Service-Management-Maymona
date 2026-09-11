using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models.Enums;
using SmartServiceManagement.ViewModels;

namespace SmartServiceManagement.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { _context = context; _userManager = userManager; }
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var provider = await _context.Providers.Include(p => p.Services).FirstOrDefaultAsync(p => p.UserId == userId);
            if (provider == null) return RedirectToAction("Apply", "ProviderRequest", new { area = "" });
            var requests = _context.ServiceRequests.Where(r => r.Service.ProviderId == provider.ProviderId);
            var model = new ProviderDashboardViewModel
            {
                Provider = provider,
                TotalServices = await _context.Services.CountAsync(s => s.ProviderId == provider.ProviderId),
                TotalRequests = await requests.CountAsync(),
                PendingRequests = await requests.CountAsync(r => r.Status == RequestStatus.Pending),
                InProgressRequests = await requests.CountAsync(r => r.Status == RequestStatus.InProgress),
                CompletedRequests = await requests.CountAsync(r => r.Status == RequestStatus.Completed),
                UnreadNotifications = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead),
                RecentRequests = await requests.Include(r => r.Customer).Include(r => r.Service).OrderByDescending(r => r.RequestDate).Take(10).ToListAsync()
            };
            return View(model);
        }
    }
}
