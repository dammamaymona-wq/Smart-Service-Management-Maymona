using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SmartServiceManagement.ViewModels
{
    public class ServiceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الخدمة مطلوب")]
        [Display(Name = "اسم الخدمة")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0.01, double.MaxValue, ErrorMessage = "يجب أن يكون السعر أكبر من صفر")]
        [Display(Name = "السعر")]
        public decimal Price { get; set; }

        [Display(Name = "الحالة")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "صورة الخدمة الحاليّة")]
        public string? ImagePath { get; set; }

        [Display(Name = "رفع صورة جديدة")]
        public IFormFile? ImageFile { get; set; }
    }
}