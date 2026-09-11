using System.ComponentModel.DataAnnotations;

namespace SmartServiceManagement.ViewModels
{
    public class RoleViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "اسم الدور مطلوب")]
        [Display(Name = "اسم الدور")]
        public string RoleName { get; set; } = string.Empty;
    }
}