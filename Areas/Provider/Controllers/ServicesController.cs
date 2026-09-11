using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;
using SmartServiceManagement.ViewModels;

namespace SmartServiceManagement.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        public ServicesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment) { _context = context; _userManager = userManager; _environment = environment; }
        private Task<global::SmartServiceManagement.Models.Provider?> CurrentProvider() => _context.Providers.FirstOrDefaultAsync(p => p.UserId == _userManager.GetUserId(User));
        public async Task<IActionResult> Index() { var p = await CurrentProvider(); if (p == null) return Forbid(); return View(await _context.Services.Include(s => s.Category).Where(s => s.ProviderId == p.ProviderId).OrderByDescending(s => s.CreatedAt).ToListAsync()); }
        [HttpGet]
        public async Task<IActionResult> Create() { ViewBag.Categories = new SelectList(await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(), "CategoryId", "CategoryName"); return View(); }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model, IFormFile? imageFile)
        {
            var p = await CurrentProvider(); if (p == null) return Forbid(); ModelState.Remove(nameof(Service.Provider)); ModelState.Remove(nameof(Service.Category));
            if (!ModelState.IsValid) { ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "CategoryName", model.CategoryId); return View(model); }
            model.ProviderId = p.ProviderId; model.CreatedAt = DateTime.UtcNow; model.Image = await SaveImageAsync(imageFile, "services"); _context.Services.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id) { var p = await CurrentProvider(); var service = await _context.Services.FirstOrDefaultAsync(s => s.ServiceId == id && s.ProviderId == p!.ProviderId); if (service == null) return NotFound(); service.IsActive = !service.IsActive; service.UpdatedAt = DateTime.UtcNow; await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        private async Task<string?> SaveImageAsync(IFormFile? file, string folder) { if (file is not { Length: > 0 }) return null; var ext = Path.GetExtension(file.FileName).ToLowerInvariant(); if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext)) return null; var dir = Path.Combine(_environment.WebRootPath, "uploads", folder); Directory.CreateDirectory(dir); var name = $"{Guid.NewGuid():N}{ext}"; await using var stream = new FileStream(Path.Combine(dir, name), FileMode.Create); await file.CopyToAsync(stream); return $"/uploads/{folder}/{name}"; }
    }
}
