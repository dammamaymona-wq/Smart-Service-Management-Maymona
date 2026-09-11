using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Data;
using SmartServiceManagement.Models;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;


    public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Settings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSettings();

                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        // POST: /Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SystemSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return View(settings);
            }

            var currentSettings =
                await _context.SystemSettings.FirstOrDefaultAsync();

            if (currentSettings == null)
            {
                settings.CreatedAt = DateTime.UtcNow;

                _context.SystemSettings.Add(settings);
            }
            else
            {
                currentSettings.SystemName = settings.SystemName;
                currentSettings.Logo = settings.Logo;
                currentSettings.ContactEmail = settings.ContactEmail;
                currentSettings.ContactPhone = settings.ContactPhone;
                currentSettings.ContactAddress = settings.ContactAddress;
                currentSettings.TimeZone = settings.TimeZone;
                currentSettings.DateFormat = settings.DateFormat;

                currentSettings.Currency = settings.Currency;
                currentSettings.DefaultRequestStatus =
                    settings.DefaultRequestStatus;
                currentSettings.MaxUploadSize =
                    settings.MaxUploadSize;
                currentSettings.AllowedFileTypes =
                    settings.AllowedFileTypes;

                currentSettings.EnableEmailNotifications =
                    settings.EnableEmailNotifications;
                currentSettings.SmtpHost = settings.SmtpHost;
                currentSettings.SmtpPort = settings.SmtpPort;
                currentSettings.SenderEmail = settings.SenderEmail;
                currentSettings.SmtpUsername = settings.SmtpUsername;

                currentSettings.AllowCustomerRegistration =
                    settings.AllowCustomerRegistration;
                currentSettings.DefaultRegistrationRole =
                    settings.DefaultRegistrationRole;

                currentSettings.MaintenanceMode =
                    settings.MaintenanceMode;
                currentSettings.MaintenanceMessage =
                    settings.MaintenanceMessage;

                currentSettings.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "تم حفظ إعدادات النظام بنجاح.";

            return RedirectToAction(nameof(Index));
        }
    }


}
