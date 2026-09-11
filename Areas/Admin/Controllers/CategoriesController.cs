
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;
using Microsoft.AspNetCore.Hosting;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CategoriesController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =====================================================
        // 1. عرض جميع الأقسام
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Services)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(categories);
        }


        // =====================================================
        // 2. عرض تفاصيل القسم
        // =====================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        // =====================================================
        // 3. شاشة إضافة قسم - GET
        // =====================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // =====================================================
        // 4. إضافة قسم - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    Category model,
    IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ================================
            // رفع صورة القسم
            // ================================

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                     _environment.WebRootPath,
                    "uploads",
                    "categories"
                );
                // إنشاء المجلد إذا لم يكن موجودًا
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // اسم فريد للصورة
                var fileName = Guid.NewGuid().ToString()
                               + Path.GetExtension(imageFile.FileName);

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );

                // حفظ الصورة
                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // حفظ مسار الصورة في قاعدة البيانات
                model.Image = "/uploads/categories/" + fileName;
            }

            // تاريخ الإنشاء
            model.CreatedAt = DateTime.UtcNow;

            // إضافة القسم
            _context.Categories.Add(model);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة القسم بنجاح";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // 5. شاشة تعديل القسم - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        // =====================================================
        // 6. تعديل القسم - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
     int id,
     Category model,
     IFormFile? imageFile)
        {
            if (id != model.CategoryId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }


            // حفظ البيانات الجديدة
            category.CategoryName = model.CategoryName;
            category.Description = model.Description;
            category.UpdatedAt = DateTime.UtcNow;


            // ==========================================
            // تغيير صورة القسم إذا تم اختيار صورة جديدة
            // ==========================================

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "categories"
                );

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                // حذف الصورة القديمة
                if (!string.IsNullOrEmpty(category.Image))
                {
                    var oldImagePath = Path.Combine(
                        _environment.WebRootPath,
                        category.Image.TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar)
                    );

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }


                // إنشاء اسم جديد للصورة
                var fileName = Guid.NewGuid().ToString()
                               + Path.GetExtension(imageFile.FileName);

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );


                // حفظ الصورة الجديدة
                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }


                // تحديث مسار الصورة
                category.Image = "/uploads/categories/" + fileName;
            }


            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تعديل القسم بنجاح";

            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // 7. حذف القسم - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        // =====================================================
        // 8. تأكيد حذف القسم - POST
        // =====================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }


            // منع حذف القسم إذا كان يحتوي على خدمات
            if (category.Services.Any())
            {
                TempData["Error"] =
                    "لا يمكن حذف هذا القسم لأنه يحتوي على خدمات مرتبطة به.";

                return RedirectToAction(nameof(Index));
            }


            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف القسم بنجاح";

            return RedirectToAction(nameof(Index));
        }
    }
}

