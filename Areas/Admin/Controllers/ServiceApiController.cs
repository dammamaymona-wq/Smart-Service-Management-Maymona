using Microsoft.AspNetCore.Mvc;
using SmartServiceManagement.Models;
using System.Net;

namespace SmartServiceManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceApiController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ServiceApiController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetAPIClient()
        {
            var client = _httpClientFactory.CreateClient("SmartServiceManagementAPI");
            var token = HttpContext.Session.GetString("Access Token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        /// <summary>
        /// دالة عامة لمعالجة أخطاء الـ HTTP Status Codes وإرجاع الـ ActionResult المناسب
        /// </summary>
        private async Task<IActionResult?> HandleHttpResponseAsync<T>(HttpResponseMessage response, T? model = default)
        {
            if (response.IsSuccessStatusCode)
            {
                return null; // لا يوجد خطأ
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized: // 401
                case HttpStatusCode.Forbidden:    // 403
                    return RedirectToAction("Login", "User", new { area = "identity" });

                case HttpStatusCode.BadRequest:   // 400
                    var badRequestError = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"بيانات الإدخال غير صحيحة: {badRequestError}");
                    return model != null ? View(model) : View();

                case HttpStatusCode.NotFound:     // 404
                    ModelState.AddModelError("", "الرابط أو العنصر المطلوب غير موجود.");
                    return model != null ? View(model) : View("NotFound");

                case HttpStatusCode.Conflict:     // 409
                    ModelState.AddModelError("", "الخدمة موجودة بالفعل أو توجد بيانات متضاربة.");
                    return model != null ? View(model) : View();

                case HttpStatusCode.InternalServerError: // 500
                    ModelState.AddModelError("", "حدث خطأ داخلي في الخادم، يرجى المحاولة لاحقاً.");
                    TempData["ErrorMessage"] = "حدث خطأ في الخادم، يرجى المحاولة لاحقاً.";
                    return model != null ? View(model) : View(new List<ServiceApi>());

                default: // أي حالة أخرى
                    ModelState.AddModelError("", $"حدث خطأ غير متوقع برقم: {(int)response.StatusCode}");
                    TempData["ErrorMessage"] = $"حدث خطأ غير متوقع: {response.StatusCode}";
                    return model != null ? View(model) : View(new List<ServiceApi>());
            }
        }

        public async Task<IActionResult> Index()
        {
            var client = GetAPIClient();
            var response = await client.GetAsync("api/Services");

            // استدعاء دالة الفحص للتحقق من الأخطاء
            var errorResult = await HandleHttpResponseAsync<List<ServiceApi>>(response);
            if (errorResult != null)
            {
                return errorResult;
            }

            var services = await response.Content.ReadFromJsonAsync<List<ServiceApi>>();
            return View(services ?? new List<ServiceApi>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceApi model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = GetAPIClient();
            var response = await client.PostAsJsonAsync("api/Services", model);

            // استدعاء دالة الفحص للتحقق من الأخطاء
            var errorResult = await HandleHttpResponseAsync(response, model);
            if (errorResult != null)
            {
                return errorResult;
            }

            TempData["SuccessMessage"] = "تم إنشاء الخدمة بنجاح.";
            return RedirectToAction(nameof(Index));
        }
    }
}