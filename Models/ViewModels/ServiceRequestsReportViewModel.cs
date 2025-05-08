using System.Collections.Generic;

namespace HomeownersAssociation.Models.ViewModels
{
    public class ServiceRequestsReportViewModel
    {
        public int TotalRequests { get; set; }
        public Dictionary<string, int> RequestsByStatus { get; set; }
        public Dictionary<string, int> RequestsByCategory { get; set; }
        public Dictionary<string, int> RequestsByPriority { get; set; }
        // Optional: Add properties for date range filtering if implemented
        // public DateTime? StartDate { get; set; }
        // public DateTime? EndDate { get; set; }

        public ServiceRequestsReportViewModel()
        {
            RequestsByStatus = new Dictionary<string, int>();
            RequestsByCategory = new Dictionary<string, int>();
            RequestsByPriority = new Dictionary<string, int>();
        }
    }
} 