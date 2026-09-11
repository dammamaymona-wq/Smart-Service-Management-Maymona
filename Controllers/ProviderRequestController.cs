using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;
using SmartServiceManagement.Models.Enums;
using SmartServiceManagement.Services;

namespace SmartServiceManagement.Controllers
{
    [Authorize] // يتطلب تسجيل دخول المستخدم
    public class ProviderRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly INotificationService _notificationService;

        public ProviderRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _notificationService = notificationService;
        }
        // GET: /ProviderRequest/Apply
        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // التحقق مما إذا كان للمستخدم طلب سابق
            var existingProvider = await _context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProvider != null)
            {
                // بدلاً من التحويل للـ Home، نرجع صفحة توضح حالة طلبه الحالي
                return View("AlreadyApplied", existingProvider);
            }

            return View();
        }
        // POST: /ProviderRequest/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(Provider provider, IFormFile? logoFile)
        {
            var userId = _userManager.GetUserId(User);

            // استثناء الخصائص التي سنقوم بتعيينها برمجياً وليس من الفورم
            ModelState.Remove(nameof(provider.UserId));
            ModelState.Remove(nameof(provider.User));

            if (!ModelState.IsValid)
            {
                return View(provider);
            }

            // ضبط القيم المطلوبة لطلب جديد
            provider.UserId = userId!;
            provider.Status = ProviderStatus.Pending; // تعيين الحالة كمعلق
            provider.CreatedAt = DateTime.UtcNow;
            provider.IsAvailable = false;

            if (logoFile is { Length: > 0 })
            {
                var extension = Path.GetExtension(logoFile.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, "صيغة الشعار غير مدعومة. استخدم JPG أو PNG أو WEBP.");
                    return View(provider);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "providers");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid():N}{extension}";
                await using var stream = new FileStream(Path.Combine(uploadsFolder, fileName), FileMode.Create);
                await logoFile.CopyToAsync(stream);
                provider.Logo = $"/uploads/providers/{fileName}";
            }

            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();

            var administrators = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var administrator in administrators)
            {
                await _notificationService.SendNotificationAsync(
                    administrator.Id,
                    "طلب مزود خدمة جديد",
                    $"تقدم {provider.CompanyName} بطلب انضمام جديد ويحتاج إلى مراجعة.");
            }

            TempData["SuccessMessage"] = "تم إرسال طلبك بنجاح! سيتم مراجعته من قبل إدارة الموقع.";
            return RedirectToAction("Index", "Home");
        }
    }
}
