using Microsoft.AspNetCore.Identity;
using SmartServiceManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartServiceManagement.Areas.Identity.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Address { get; set; }

        public string? ProfileImage { get; set; }

        public virtual SmartServiceManagement.Models.Provider? Provider { get; set; } // بحدد اسمها بالكامل لانه بتخربط مع النيم سبيس يلي عندي لانها نفس الاسم 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}