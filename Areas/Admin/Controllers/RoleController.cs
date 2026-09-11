using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models; // مسار ApplicationUser الخاص بمشروعك
using SmartServiceManagement.ViewModels;

namespace SmartServiceManagement.Controllers
{
    // حماية الـ Controller ليكون متاحاً فقط لمن يملك دور Admin

    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // 1. عرض جدول جميع الأدوار المتاحة بالنظام
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        // 2. شاشة إضافة دور جديد (GET)
        public IActionResult Create()
        {
            return View();
        }

        // 3. حفظ الدور الجديد (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var roleExist = await _roleManager.RoleExistsAsync(model.RoleName);
                if (!roleExist)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "تم إنشاء الدور بنجاح";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "هذا الدور موجود بالفعل");
                }
            }
            return View(model);
        }

        // 4. عرض شاشة تعيين الأدوار لمستخدم معين (GET)
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? ""
            };

            var roles = await _roleManager.Roles.ToListAsync();
            foreach (var role in roles)
            {
                viewModel.Roles.Add(new RoleSelection
                {
                    RoleName = role.Name ?? "",
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name ?? "")
                });
            }

            return View(viewModel);
        }

        // 5. حفظ الأدوار المحددة للمستخدم (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            // إزالة كافة الأدوار السابقة
            var userRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إزالة الأدوار القديمة");
                return View(model);
            }

            // إضافة الأدوار المختارة من الواجهة
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);

            if (addResult.Succeeded)
            {
                TempData["Success"] = "تم تحديث أدوار المستخدم بنجاح";
                return RedirectToAction("Index", "User");
            }

            ModelState.AddModelError("", "حدث خطأ أثناء إضافة الأدوار الجديدة");
            return View(model);
        }
    }
}