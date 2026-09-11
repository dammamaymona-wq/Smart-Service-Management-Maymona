using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;
using SmartServiceManagement.Models.Enums;
using SmartServiceManagement.Services;
using System.Security.Claims;

namespace SmartServiceManagement.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public ServiceRequestsController(
            ApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: /ServiceRequests/Create?serviceId=5
        public async Task<IActionResult> Create(int serviceId)
        {
            var service = await _context.Services
                .Include(s => s.Provider)
                .FirstOrDefaultAsync(s =>
                    s.ServiceId == serviceId &&
                    s.IsActive);

            if (service == null)
            {
                return NotFound("الخدمة غير موجودة أو غير مفعلة.");
            }

            var request = new ServiceRequest
            {
                ServiceId = serviceId
            };

            return View(request);
        }
        // POST: /ServiceRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest model)
        {
            // الحصول على المستخدم الحالي
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // إذا لم يتم العثور على المستخدم
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] =
                    "لم يتم التعرف على المستخدم المسجل الدخول.";

                return RedirectToAction(
                    nameof(Create),
                    new { serviceId = model.ServiceId });
            }

            // البحث عن الخدمة
            var service = await _context.Services
                .Include(s => s.Provider)
                .FirstOrDefaultAsync(s =>
                    s.ServiceId == model.ServiceId &&
                    s.IsActive);

            if (service == null)
            {
                return NotFound(
                    "الخدمة غير موجودة أو غير مفعلة.");
            }

            // التأكد من وجود مزود للخدمة
            if (service.Provider == null)
            {
                return NotFound(
                    "مزود الخدمة غير موجود.");
            }

            // هذه البيانات يتم تحديدها من السيرفر
            // وليس من صفحة Create
            ModelState.Remove(
                nameof(ServiceRequest.CustomerId));

            ModelState.Remove(
                nameof(ServiceRequest.RequestDate));

            ModelState.Remove(
                nameof(ServiceRequest.Customer));

            ModelState.Remove(
                nameof(ServiceRequest.Service));

            // التحقق من صحة البيانات
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value != null)
                    .SelectMany(x => x.Value!.Errors
                        .Select(e =>
                            $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();

                TempData["ErrorMessage"] =
                    "بيانات الطلب غير صحيحة: " +
                    string.Join(" | ", errors);

                return View(model);
            }

            // إنشاء الطلب
            var request = new ServiceRequest
            {
                CustomerId = userId,

                ServiceId = model.ServiceId,

                PreferredDate = model.PreferredDate,

                ProblemDescription =
                    model.ProblemDescription,

                CustomerAddress =
                    model.CustomerAddress,

                RequestDate = DateTime.UtcNow,

                Status = RequestStatus.Pending,

                IsCompleted = false
            };

            _context.ServiceRequests.Add(request);

            // حفظ الطلب أولًا
            await _context.SaveChangesAsync();


            // =====================================================
            // 🔔 إرسال إشعار إلى مزود الخدمة
            // =====================================================

            if (!string.IsNullOrEmpty(service.Provider.UserId))
            {
                await _notificationService.SendNotificationAsync(
                    service.Provider.UserId,
                    "طلب خدمة جديد",
                    $"لديك طلب خدمة جديد رقم #{request.RequestId}."
                );
            }


            // رسالة نجاح
            TempData["SuccessMessage"] =
                "تم إرسال طلب الخدمة بنجاح.";


            // الانتقال إلى طلبات العميل بعد الحفظ
            return RedirectToAction(
                "Index",
                "ServiceRequests",
                new { area = "Customer" });
        }

    }
}
