using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartServiceManagement.Areas.Identity.Models;

namespace SmartServiceManagement.Models
{
    public class Provider
    {
        [Key]
        public int ProviderId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int ExperienceYears { get; set; }

        [Phone]
        public string? ContactNumber { get; set; }

        [StringLength(150)]
        public string? City { get; set; }

        public bool IsAvailable { get; set; } = false;   // public ProviderStatus Status { get; set; } = ProviderStatus.Pending; بدل ال 
        public SmartServiceManagement.Models.Enums.ProviderStatus Status { get; set; }
    = SmartServiceManagement.Models.Enums.ProviderStatus.Pending;
        public double AverageRating { get; set; } = 0;

        public string? Logo { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}