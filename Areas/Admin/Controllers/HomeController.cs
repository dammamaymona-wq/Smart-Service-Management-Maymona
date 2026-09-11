using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models.Enums;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
        [Authorize(Roles = "Admin,Manager")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // إجمالي طلبات الخدمة
            ViewBag.TotalRequests = await _context.ServiceRequests.CountAsync();

            // الطلبات قيد الانتظار
            ViewBag.PendingRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == RequestStatus.Pending);

            // الطلبات قيد التنفيذ
            ViewBag.InProgressRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == RequestStatus.InProgress);

            // الطلبات المكتملة
            ViewBag.CompletedRequests = await _context.ServiceRequests
                .CountAsync(x => x.Status == RequestStatus.Completed);

            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProviders = await _context.Providers.CountAsync(x => x.Status == ProviderStatus.Approved);
            ViewBag.TotalServices = await _context.Services.CountAsync(x => x.IsActive);
            ViewBag.PendingProviderRequests = await _context.Providers.CountAsync(x => x.Status == ProviderStatus.Pending);
            ViewBag.UnreadNotifications = await _context.Notifications.CountAsync(x => !x.IsRead);

            return View();
        }
    }
}
