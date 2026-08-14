using FashionHubWeb.Models;

namespace FashionHubWeb.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }

        // Chart 1: Products by Category (Doughnut Chart)
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryCounts { get; set; } = new();

        // Chart 2: Products by Brand (Bar Chart)
        public List<string> BrandLabels { get; set; } = new();
        public List<int> BrandCounts { get; set; } = new();

        // Chart 3: Order Status Breakdown (Pie Chart)
        public List<string> OrderStatusLabels { get; set; } = new();
        public List<int> OrderStatusCounts { get; set; } = new();

        // Chart 4: Monthly Revenue Trend (Line Chart)
        public List<string> RevenueLabels { get; set; } = new();
        public List<decimal> RevenueValues { get; set; } = new();

        public List<Order> RecentOrders { get; set; } = new();
    }
}
