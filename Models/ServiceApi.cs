using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartServiceManagement.Models
{
    public class ServiceApi
    {
        public int ServiceId { get; set; }

        [Required]
        [StringLength(150)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public string? Image { get; set; }

        public int EstimatedDuration { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
