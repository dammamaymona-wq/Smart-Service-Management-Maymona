using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;
using SmartServiceManagement.Models.Enums;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Services;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
     [Authorize(Roles = "Admin")]
    public class ProvidersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public ProvidersController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // =====================================================
        // 1. عرض جميع مزودي الخدمة المقبولين
        // =====================================================
        public async Task<IActionResult> Index()
        {
            var providers = await _context.Providers
                .Include(p => p.User)
                .Include(p => p.Services)
                .Where(p => p.Status == ProviderStatus.Approved)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(providers);
        }

        // =====================================================
        // 2. عرض الطلبات المعلقة
        // =====================================================
        public async Task<IActionResult> Requests()
        {
            var requests = await _context.Providers
                .Include(p => p.User)
                .Where(p => p.Status == ProviderStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // =====================================================
        // 3. عرض تفاصيل مزود الخدمة
        // =====================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var provider = await _context.Providers
                .Include(p => p.User)
                .Include(p => p.Services)
                .FirstOrDefaultAsync(p => p.ProviderId == id);

            if (provider == null)
            {
                return NotFound();
            }

            return View(provider);
        }

        // =====================================================
        // 4. الموافقة على طلب مزود الخدمة (مع إضافة الـ Role)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var provider = await _context.Providers
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProviderId == id);

            if (provider == null)
            {
                return NotFound();
            }

            // التأكد أن الطلب فعلاً معلق
            if (provider.Status != ProviderStatus.Pending)
            {
                TempData["Success"] = "هذا الطلب تمت معالجته مسبقاً";
                return RedirectToAction(nameof(Requests));
            }

            // تغيير حالة الطلب إلى موافق عليه وتفعيله
            provider.Status = ProviderStatus.Approved;
            provider.IsAvailable = true;
            provider.UpdatedAt = DateTime.UtcNow;

            // إضافة المستخدم إلى Role Provider
            if (provider.User != null)
            {
                if (!await _userManager.IsInRoleAsync(provider.User, "Provider"))
                {
                    await _userManager.AddToRoleAsync(provider.User, "Provider");
                }
            }

            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                provider.UserId,
                "تم قبول طلب مزود الخدمة",
                "تمت الموافقة على طلبك، يمكنك الآن إدارة خدماتك واستقبال الطلبات.");

            TempData["Success"] = "تمت الموافقة على طلب مزود الخدمة بنجاح ونقله إلى قائمة المزودين.";
            return RedirectToAction(nameof(Requests));
        }

        // =====================================================
        // 5. رفض طلب مزود الخدمة
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.ProviderId == id);

            if (provider == null)
            {
                return NotFound();
            }

            if (provider.Status != ProviderStatus.Pending)
            {
                TempData["Success"] = "هذا الطلب تمت معالجته مسبقاً";
                return RedirectToAction(nameof(Requests));
            }

            provider.Status = ProviderStatus.Rejected;
            provider.IsAvailable = false;
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(
                provider.UserId,
                "تحديث على طلب مزود الخدمة",
                "نعتذر، تم رفض طلب الانضمام حالياً. يمكنك تحديث بياناتك وإعادة التقديم لاحقاً.");

            TempData["Success"] = "تم رفض طلب مزود الخدمة.";
            return RedirectToAction(nameof(Requests));
        }

        // =====================================================
        // 6. تفعيل مزود الخدمة
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.ProviderId == id);

            if (provider == null)
            {
                return NotFound();
            }

            provider.IsAvailable = true;
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تفعيل مزود الخدمة بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // 7. إيقاف / تعطيل مزود الخدمة
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.ProviderId == id);

            if (provider == null)
            {
                return NotFound();
            }

            provider.IsAvailable = false;
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إيقاف مزود الخدمة بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // 8. شاشة تعديل مزود الخدمة - GET
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var provider = await _context.Providers
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProviderId == id);

            if (provider == null)
            {
                return NotFound();
            }

            return View(provider);
        }

        // =====================================================
        // 9. تعديل مزود الخدمة - POST (مع رفع الشعار)
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    global::SmartServiceManagement.Models.Provider model,
    IFormFile? logoFile)
        {
            if (model.ProviderId <= 0)
            {
                return NotFound();
            }

            // استثناء Navigation Properties من الـ Validation
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.ProviderId == model.ProviderId);

            if (provider == null)
            {
                return NotFound();
            }

            // تحديث بيانات المزود
            provider.CompanyName = model.CompanyName;
            provider.Description = model.Description;
            provider.ExperienceYears = model.ExperienceYears;
            provider.ContactNumber = model.ContactNumber;
            provider.City = model.City;
            provider.IsAvailable = model.IsAvailable;
            provider.UpdatedAt = DateTime.UtcNow;

            // معالجة رفع الشعار الجديد إن وجد
            if (logoFile != null && logoFile.Length > 0)
            {
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    return BadRequest("لم يتم العثور على مجلد wwwroot.");
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "providers");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(logoFile.FileName);
                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }

                provider.Logo = "/uploads/providers/" + fileName;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تعديل بيانات مزود الخدمة بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }
}
