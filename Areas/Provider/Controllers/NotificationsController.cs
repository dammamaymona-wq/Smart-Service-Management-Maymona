using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;

namespace SmartServiceManagement.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context; private readonly UserManager<ApplicationUser> _userManager;
        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) { _context = context; _userManager = userManager; }
        public async Task<IActionResult> Index() => View(await _context.Notifications.Where(n => n.UserId == _userManager.GetUserId(User)).OrderByDescending(n => n.CreatedAt).ToListAsync());
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id) { var n = await _context.Notifications.FirstOrDefaultAsync(x => x.NotificationId == id && x.UserId == _userManager.GetUserId(User)); if (n != null) { n.IsRead = true; await _context.SaveChangesAsync(); } return RedirectToAction(nameof(Index)); }
    }
}
