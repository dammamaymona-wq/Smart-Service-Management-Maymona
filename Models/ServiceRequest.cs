using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartServiceManagement.Areas.Identity.Models;
using SmartServiceManagement.Models.Enums;

namespace SmartServiceManagement.Models
{
    public class ServiceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [ForeignKey(nameof(CustomerId))]
        public virtual ApplicationUser Customer { get; set; } = null!;

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey(nameof(ServiceId))]
        public virtual Service Service { get; set; } = null!;

        [Required]
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? PreferredDate { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [StringLength(1000)]
        public string? ProblemDescription { get; set; }

        [StringLength(250)]
        public string? CustomerAddress { get; set; }

        public bool IsCompleted { get; set; } = false;
    }
}