using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Data;
using SmartServiceManagement.Services;
using System.Linq;

namespace SmartServiceManagement
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder =
                WebApplication.CreateBuilder(args);

            // =====================================================
            // 1. الاتصال بقاعدة البيانات
            // =====================================================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString(
                        "BloggingDatabase"
                    )
                )
            );

            // =====================================================
            // 2. إعداد ASP.NET Core Identity والأدوار
            // =====================================================
            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;

                    options.User.RequireUniqueEmail = true;

                    // إعدادات تسجيل الدخول
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // =====================================================
            // 3. إضافة MVC
            // =====================================================
            builder.Services.AddControllersWithViews();

            // =====================================================
            // 4. تسجيل الخدمات الخاصة بالنظام
            // =====================================================
            builder.Services.AddScoped<
                INotificationService,
                NotificationService
            >();

            // =====================================================
            // 5. إعداد HttpClient للـ API الخارجية
            // =====================================================
            var apiBaseUrl =
                builder.Configuration["ApiSettings:BaseUrl"];

            if (!string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                builder.Services.AddHttpClient(
                    "SocialXApi",
                    client =>
                    {
                        client.BaseAddress =
                            new Uri(apiBaseUrl);
                    }
                );
            }

            var app = builder.Build();

            // =====================================================
            // 6. إنشاء قاعدة البيانات والأدوار وحساب الأدمن
            // =====================================================
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var db =
                        services.GetRequiredService<
                            ApplicationDbContext
                        >();

                    // تطبيق جميع migrations على قاعدة البيانات
                    await db.Database.MigrateAsync();

                    var roleManager =
                        services.GetRequiredService<
                            RoleManager<IdentityRole>
                        >();

                    var userManager =
                        services.GetRequiredService<
                            UserManager<ApplicationUser>
                        >();

                    // =================================================
                    // إنشاء الأدوار الأساسية
                    // =================================================
                    string[] roleNames =
                    {
                        "Admin",
                        "Manager",
                        "Customer",
                        "Provider"
                    };

                    foreach (var roleName in roleNames)
                    {
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            var roleResult =
                                await roleManager.CreateAsync(
                                    new IdentityRole(roleName)
                                );

                            if (!roleResult.Succeeded)
                            {
                                var errors = string.Join(
                                    " | ",
                                    roleResult.Errors.Select(
                                        error => error.Description
                                    )
                                );

                                throw new Exception(
                                    $"فشل إنشاء الدور {roleName}: {errors}"
                                );
                            }
                        }
                    }

                    // =================================================
                    // بيانات حساب الأدمن
                    // =================================================
                    var adminEmail =
                        "admin@smartservice.com";

                    var adminPassword =
                        "Admin123!";

                    // البحث عن حساب الأدمن
                    var adminUser =
                        await userManager.FindByEmailAsync(
                            adminEmail
                        );

                    // =================================================
                    // إنشاء حساب الأدمن إذا لم يكن موجوداً
                    // =================================================
                    if (adminUser == null)
                    {
                        adminUser = new ApplicationUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true,
                            FullName = "System Administrator",
                            CreatedAt = DateTime.UtcNow
                        };

                        var createResult =
                            await userManager.CreateAsync(
                                adminUser,
                                adminPassword
                            );

                        if (!createResult.Succeeded)
                        {
                            var errors = string.Join(
                                " | ",
                                createResult.Errors.Select(
                                    error => error.Description
                                )
                            );

                            throw new Exception(
                                $"فشل إنشاء حساب الأدمن: {errors}"
                            );
                        }

                        Console.WriteLine(
                            "تم إنشاء حساب الأدمن بنجاح."
                        );
                    }

                    // =================================================
                    // التأكد من ربط الحساب بدور Admin
                    // =================================================
                    if (!await userManager.IsInRoleAsync(
                            adminUser,
                            "Admin"
                        ))
                    {
                        var addRoleResult =
                            await userManager.AddToRoleAsync(
                                adminUser,
                                "Admin"
                            );

                        if (!addRoleResult.Succeeded)
                        {
                            var errors = string.Join(
                                " | ",
                                addRoleResult.Errors.Select(
                                    error => error.Description
                                )
                            );

                            throw new Exception(
                                $"فشل ربط المستخدم بدور Admin: {errors}"
                            );
                        }

                        Console.WriteLine(
                            "تم ربط حساب الأدمن بدور Admin بنجاح."
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            "حساب الأدمن مربوط مسبقاً بدور Admin."
                        );
                    }

                    Console.WriteLine(
                        $"حساب الأدمن جاهز: {adminEmail}"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        "حدث خطأ أثناء تهيئة قاعدة البيانات والأدوار:"
                    );

                    Console.WriteLine(ex.ToString());
                }
            }

            // =====================================================
            // 7. إعداد HTTP Request Pipeline
            // =====================================================
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            // يجب أن يأتي Authentication قبل Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // =====================================================
            // 8. Route الخاصة بالـ Areas
            // =====================================================
            app.MapControllerRoute(
                name: "areas",
                pattern:
                    "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            );

            // =====================================================
            // 9. Route العامة للموقع
            // =====================================================
            app.MapControllerRoute(
                name: "default",
                pattern:
                    "{controller=Home}/{action=Index}/{id?}"
            );

            // =====================================================
            // 10. تشغيل التطبيق
            // =====================================================
            app.Run();
        }
    }
}
