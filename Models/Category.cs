using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartServiceManagement.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public string? Image { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}