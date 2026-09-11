using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;
using SmartServiceManagement.ViewModels;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServicesController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==================== INDEX ====================
        // GET: Admin/Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Select(s => new ServiceViewModel
                {
                    Id = s.ServiceId,
                    Title = s.ServiceName,
                    Description = s.Description,
                    Price = s.Price,
                    IsActive = s.IsActive,
                    ImagePath = s.Image
                })
                .ToListAsync();

            return View(services);
        }


        // ==================== CREATE ====================
        // GET: Admin/Services/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId",
                "CategoryName");

            ViewBag.Providers = new SelectList(
                await _context.Providers.ToListAsync(),
                "ProviderId",
                "CompanyName");

            return View();
        }


        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceViewModel model,
            int categoryId,
            int providerId,
            int estimatedDuration)
        {
            if (ModelState.IsValid)
            {
                string? imageFileName = null;

                if (model.ImageFile != null)
                {
                    imageFileName =
                        await SaveUploadedImageAsync(model.ImageFile);
                }

                var service = new Service
                {
                    ServiceName = model.Title,
                    Description = model.Description ?? string.Empty,
                    Price = model.Price,
                    IsActive = model.IsActive,

                    Image = imageFileName != null
                        ? "/uploads/services/" + imageFileName
                        : null,

                    CategoryId = categoryId,
                    ProviderId = providerId,
                    EstimatedDuration = estimatedDuration,

                    CreatedAt = DateTime.UtcNow
                };

                _context.Services.Add(service);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId",
                "CategoryName",
                categoryId);

            ViewBag.Providers = new SelectList(
                await _context.Providers.ToListAsync(),
                "ProviderId",
                "CompanyName",
                providerId);

            return View(model);
        }


        // ==================== EDIT ====================
        // GET: Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.Services.FindAsync(id);

            if (service == null)
                return NotFound();

            var viewModel = new ServiceViewModel
            {
                Id = service.ServiceId,
                Title = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                IsActive = service.IsActive,
                ImagePath = service.Image
            };

            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId",
                "CategoryName",
                service.CategoryId);

            ViewBag.Providers = new SelectList(
                await _context.Providers.ToListAsync(),
                "ProviderId",
                "CompanyName",
                service.ProviderId);

            ViewBag.EstimatedDuration =
                service.EstimatedDuration;

            return View(viewModel);
        }


        // POST: Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ServiceViewModel model,
            int categoryId,
            int providerId,
            int estimatedDuration,
            bool removeExistingImage)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var service =
                    await _context.Services.FindAsync(id);

                if (service == null)
                    return NotFound();


                // 1. حذف الصورة الحالية
                if (removeExistingImage &&
                    !string.IsNullOrEmpty(service.Image))
                {
                    DeleteImageFile(service.Image);

                    service.Image = null;
                }


                // 2. رفع صورة جديدة واستبدال القديمة
                if (model.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(service.Image))
                    {
                        DeleteImageFile(service.Image);
                    }

                    string newFileName =
                        await SaveUploadedImageAsync(model.ImageFile);

                    service.Image =
                        "/uploads/services/" + newFileName;
                }


                // 3. تحديث البيانات
                service.ServiceName =
                    model.Title;

                service.Description =
                    model.Description ?? string.Empty;

                service.Price =
                    model.Price;

                service.IsActive =
                    model.IsActive;

                service.CategoryId =
                    categoryId;

                service.ProviderId =
                    providerId;

                service.EstimatedDuration =
                    estimatedDuration;

                service.UpdatedAt =
                    DateTime.UtcNow;


                _context.Update(service);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            ViewBag.Categories = new SelectList(
                await _context.Categories.ToListAsync(),
                "CategoryId",
                "CategoryName",
                categoryId);

            ViewBag.Providers = new SelectList(
                await _context.Providers.ToListAsync(),
                "ProviderId",
                "CompanyName",
                providerId);

            ViewBag.EstimatedDuration =
                estimatedDuration;

            return View(model);
        }


        // ==================== DELETE ====================
        // GET: Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var service = await _context.Services
                .Include(s => s.Category)
                .Include(s => s.Provider)
                .FirstOrDefaultAsync(
                    m => m.ServiceId == id);


            if (service == null)
                return NotFound();


            // التحقق من وجود طلبات مرتبطة بالخدمة
            var hasRequests =
                await _context.ServiceRequests
                    .AnyAsync(r => r.ServiceId == id);


            var viewModel = new ServiceViewModel
            {
                Id = service.ServiceId,
                Title = service.ServiceName,
                Description = service.Description,
                Price = service.Price,
                IsActive = service.IsActive,
                ImagePath = service.Image
            };


            ViewBag.CategoryName =
                service.Category?.CategoryName;

            ViewBag.ProviderName =
                service.Provider?.CompanyName;


            // إرسال حالة ارتباط الخدمة بالطلبات
            // إلى صفحة Delete.cshtml
            ViewBag.HasRequests =
                hasRequests;


            return View(viewModel);
        }


        // POST: Admin/Services/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service =
                await _context.Services
                    .FirstOrDefaultAsync(
                        s => s.ServiceId == id);


            if (service == null)
                return NotFound();


            // ============================
            // التحقق من الطلبات المرتبطة
            // ============================

            var hasRequests =
                await _context.ServiceRequests
                    .AnyAsync(r => r.ServiceId == id);


            if (hasRequests)
            {
                TempData["Error"] =
                    "لا يمكن حذف هذه الخدمة لأنها مرتبطة بطلبات خدمة موجودة في النظام. يمكنك تعطيل الخدمة بدلاً من حذفها.";

                return RedirectToAction(nameof(Index));
            }


            // ============================
            // حذف الصورة
            // ============================

            if (!string.IsNullOrEmpty(service.Image))
            {
                DeleteImageFile(service.Image);
            }


            // ============================
            // حذف الخدمة
            // ============================

            _context.Services.Remove(service);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "تم حذف الخدمة بنجاح.";


            return RedirectToAction(nameof(Index));
        }


        // ==================== TOGGLE STATUS ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var service =
                await _context.Services.FindAsync(id);


            if (service == null)
                return NotFound();


            service.IsActive =
                !service.IsActive;


            _context.Update(service);

            await _context.SaveChangesAsync();


            if (service.IsActive)
            {
                TempData["Success"] =
                    "تم تفعيل الخدمة بنجاح.";
            }
            else
            {
                TempData["Success"] =
                    "تم تعطيل الخدمة بنجاح.";
            }


            return RedirectToAction(nameof(Index));
        }


        // ==================== HELPER METHODS ====================

        private async Task<string> SaveUploadedImageAsync(
            IFormFile file)
        {
            string uploadsFolder =
                Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "uploads",
                    "services");


            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(
                    uploadsFolder);
            }


            string uniqueFileName =
                Guid.NewGuid().ToString()
                + "_"
                + Path.GetFileName(file.FileName);


            string filePath =
                Path.Combine(
                    uploadsFolder,
                    uniqueFileName);


            using (var fileStream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await file.CopyToAsync(
                    fileStream);
            }


            return uniqueFileName;
        }


        private void DeleteImageFile(
            string relativePath)
        {
            string fullPath =
                Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    relativePath.TrimStart('/'));


            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(
                    fullPath);
            }
        }
    }
}