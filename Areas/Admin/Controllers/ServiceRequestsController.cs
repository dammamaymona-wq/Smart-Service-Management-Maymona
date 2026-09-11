using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;
using SmartServiceManagement.Models.Enums;
using SmartServiceManagement.Services;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Manager,Provider")]

    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ServiceRequestsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Admin/ServiceRequests
        public async Task<IActionResult> Index()
        {
            var requests = await _context.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Service)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            return View(requests);
        }

        // GET: Admin/ServiceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Service)
                    .ThenInclude(s => s.Provider)
                .Include(r => r.Service)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: Admin/ServiceRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.ServiceRequests
                .Include(r => r.Customer)
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // POST: Admin/ServiceRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            RequestStatus status)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Service)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            request.Status = status;
            request.UpdatedAt = DateTime.UtcNow;

            // تحديث IsCompleted تلقائيًا حسب الحالة
            request.IsCompleted = status == RequestStatus.Completed;

            _context.Update(request);

            await _context.SaveChangesAsync();

            // إرسال إشعار للعميل عند تحديث الحالة
            if (!string.IsNullOrEmpty(request.CustomerId))
            {
                await _notificationService.SendNotificationAsync(
                    request.CustomerId,
                    "تحديث على طلب الخدمة",
                    $"قام مدير النظام بتحديث حالة طلبك رقم #{request.RequestId} إلى: {status}"
                );
            }

            TempData["SuccessMessage"] =
                "تم تحديث حالة طلب الخدمة بنجاح.";


            return RedirectToAction(
                "Index",
                "ServiceRequests",
                new { area = "Admin" });
        }

        // POST: Admin/ServiceRequests/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            _context.ServiceRequests.Remove(request);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "تم حذف طلب الخدمة بنجاح.";

            return RedirectToAction(
                "Index",
                "ServiceRequests",
                new { area = "Admin" });
        }
    }
}
