using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models.Enums;
using SmartServiceManagement.ViewModels;

namespace SmartServiceManagement.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = "Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { _context = context; _userManager = userManager; }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User)!;
            var requests = _context.ServiceRequests.Where(r => r.CustomerId == userId);
            var model = new CustomerDashboardViewModel
            {
                TotalRequests = await requests.CountAsync(),
                PendingRequests = await requests.CountAsync(r => r.Status == RequestStatus.Pending),
                InProgressRequests = await requests.CountAsync(r => r.Status == RequestStatus.InProgress),
                CompletedRequests = await requests.CountAsync(r => r.Status == RequestStatus.Completed),
                UnreadNotifications = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead),
                RecentRequests = await requests.Include(r => r.Service).ThenInclude(s => s.Provider).OrderByDescending(r => r.RequestDate).Take(8).ToListAsync(),
                SuggestedServices = await _context.Services.Include(s => s.Provider).Include(s => s.Category).Where(s => s.IsActive && s.Provider.Status == ProviderStatus.Approved).OrderByDescending(s => s.CreatedAt).Take(6).ToListAsync()
            };
            return View(model);
        }
    }
}
