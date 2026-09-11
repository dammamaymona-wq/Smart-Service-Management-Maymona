using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models.Enums;
using SmartServiceManagement.Services;

namespace SmartServiceManagement.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context; private readonly UserManager<ApplicationUser> _userManager; private readonly INotificationService _notifications;
        public ServiceRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notifications) { _context = context; _userManager = userManager; _notifications = notifications; }
        public async Task<IActionResult> Index()
        {
            var providerId = await _context.Providers.Where(p => p.UserId == _userManager.GetUserId(User)).Select(p => (int?)p.ProviderId).FirstOrDefaultAsync();
            if (providerId == null) return Forbid();
            var items = await _context.ServiceRequests.Where(r => r.Service.ProviderId == providerId).Include(r => r.Customer).Include(r => r.Service).OrderByDescending(r => r.RequestDate).ToListAsync();
            return View(items);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, RequestStatus status)
        {
            var providerId = await _context.Providers.Where(p => p.UserId == _userManager.GetUserId(User)).Select(p => (int?)p.ProviderId).FirstOrDefaultAsync();
            var item = await _context.ServiceRequests.Include(r => r.Service).FirstOrDefaultAsync(r => r.RequestId == id && r.Service.ProviderId == providerId);
            if (item == null) return NotFound();
            item.Status = status; item.IsCompleted = status == RequestStatus.Completed; item.UpdatedAt = DateTime.UtcNow; await _context.SaveChangesAsync();
            await _notifications.SendNotificationAsync(item.CustomerId, "تحديث على طلب الخدمة", $"تم تحديث طلبك رقم #{item.RequestId} إلى الحالة: {status}.");
            return RedirectToAction(nameof(Index));
        }
    }
}
