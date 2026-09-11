using SmartServiceManagement.Models;

namespace SmartServiceManagement.ViewModels
{
    public class CustomerDashboardViewModel
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int UnreadNotifications { get; set; }
        public List<ServiceRequest> RecentRequests { get; set; } = new();
        public List<Service> SuggestedServices { get; set; } = new();
    }

    public class ProviderDashboardViewModel
    {
        public Provider Provider { get; set; } = null!;
        public int TotalServices { get; set; }
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int UnreadNotifications { get; set; }
        public List<ServiceRequest> RecentRequests { get; set; } = new();
    }
}
