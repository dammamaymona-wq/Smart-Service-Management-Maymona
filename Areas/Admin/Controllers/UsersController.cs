using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Json;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models.ViewModels;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }


        // =====================================================
        // 1. عرض جدول جميع المستخدمين
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }


        // =====================================================
        // 2. عرض تفاصيل ملف المستخدم (زر العين)
        // =====================================================

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                ProfileImage = user.ProfileImage,
                CreatedAt = user.CreatedAt,

                // حاليًا لا نستخدم UserRolesViewModel
                // لذلك نخلي Roles فارغة
                Roles = new List<string>()
            };

            return View(viewModel);
        }


        // =====================================================
        // 3. شاشة تعديل بيانات المستخدم (زر القلم - GET)
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // =====================================================
        // 4. حفظ التعديلات على البيانات (زر القلم - POST)
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            ApplicationUser model,
            IFormFile? ProfileImageFile)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (id != model.Id)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }


            // ==========================================
            // تحديث بيانات المستخدم
            // ==========================================

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.UpdatedAt = DateTime.UtcNow;


            // ==========================================
            // رفع صورة جديدة
            // ==========================================

            if (ProfileImageFile != null && ProfileImageFile.Length > 0)
            {
                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                var extension = Path
                    .GetExtension(ProfileImageFile.FileName)
                    .ToLowerInvariant();


                // التحقق من امتداد الصورة
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "ProfileImageFile",
                        "يسمح فقط بصور JPG و JPEG و PNG و WEBP."
                    );

                    return View(model);
                }


                // إنشاء اسم جديد للصورة
                var fileName = Guid.NewGuid().ToString() + extension;


                // مكان حفظ الصور
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "users"
                );


                // إنشاء المجلد إذا لم يكن موجودًا
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                // المسار الكامل للصورة
                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );


                // حفظ الصورة
                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await ProfileImageFile.CopyToAsync(stream);
                }


                // ==========================================
                // حذف الصورة القديمة
                // ==========================================

                if (!string.IsNullOrEmpty(user.ProfileImage))
                {
                    var oldImagePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        user.ProfileImage.TrimStart('/')
                    );

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }


                // حفظ رابط الصورة الجديدة
                user.ProfileImage = "/uploads/users/" + fileName;
            }


            // ==========================================
            // حفظ التعديلات
            // ==========================================

            var result = await _userManager.UpdateAsync(user);


            if (result.Succeeded)
            {
                TempData["Success"] =
                    "تم تعديل بيانات المستخدم بنجاح";

                return RedirectToAction(nameof(Index));
            }


            // عرض أخطاء Identity
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description
                );
            }

            return View(model);
        }


        // =====================================================
        // 5. تسجيل الدخول باستخدام HttpClient (GET & POST)
        // =====================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("SocialXApi");

                var response = await client.PostAsJsonAsync("api/Auth/Login", model);

                if (response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "بيانات الدخول غير صحيحة.");
            }
            catch (Exception)
            {
                throw;
            }

            return View(model);
        }

    }
}