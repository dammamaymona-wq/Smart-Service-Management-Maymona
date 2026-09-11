using System;
using System.ComponentModel.DataAnnotations;
using SmartServiceManagement.Models.Enums;

namespace SmartServiceManagement.Models
{
    public class SystemSettings
    {
        [Key]
        public int SettingsId { get; set; }

        // General Settings
        [Required]
        [StringLength(150)]
        public string SystemName { get; set; } = "Smart Service Management";

        [StringLength(250)]
        public string? Logo { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? ContactEmail { get; set; }

        [Phone]
        [StringLength(30)]
        public string? ContactPhone { get; set; }

        [StringLength(300)]
        public string? ContactAddress { get; set; }

        [StringLength(100)]
        public string TimeZone { get; set; } = "Asia/Jerusalem";

        [StringLength(30)]
        public string DateFormat { get; set; } = "dd/MM/yyyy";


        // Service & Booking Settings
        [Required]
        [StringLength(10)]
        public string Currency { get; set; } = "JOD";

        public RequestStatus DefaultRequestStatus { get; set; }
            = RequestStatus.Pending;

        [Range(1, 100)]
        public int MaxUploadSize { get; set; } = 5;

        [StringLength(500)]
        public string AllowedFileTypes { get; set; }
            = ".jpg,.jpeg,.png,.pdf";


        // Notifications / SMTP
        public bool EnableEmailNotifications { get; set; } = false;

        [StringLength(200)]
        public string? SmtpHost { get; set; }

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [EmailAddress]
        [StringLength(150)]
        public string? SenderEmail { get; set; }

        [StringLength(150)]
        public string? SmtpUsername { get; set; }


        // Account & Security
        public bool AllowCustomerRegistration { get; set; } = true;

        [StringLength(50)]
        public string DefaultRegistrationRole { get; set; } = "Customer";


        // System & Maintenance
        public bool MaintenanceMode { get; set; } = false;

        [StringLength(500)]
        public string? MaintenanceMessage { get; set; }
            = "النظام تحت الصيانة حاليًا، يرجى المحاولة لاحقًا.";


        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}