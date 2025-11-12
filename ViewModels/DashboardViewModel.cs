namespace StockManager.ViewModels
{
    public class DashboardViewModel
    {
        //Grafico mas vendidos
        public List<ProductSalesData> TopSellingProducts { get; set; } = new();
        //Grafico de productos con bajo stoc
        public List<LowStockProduct> LowStockProducts { get; set; } = new();
        //Resumen general
        public DashboardSummary Summary { get; set; } = new();
    }
    public class ProductSalesData
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public string CategoryPath { get; set; } = string.Empty;
    }
    public class LowStockProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public string Status { get; set; } = string.Empty;
        public string CategoryPath { get; set; } = string.Empty;
    }
    public class DashboardSummary
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalMovements { get; set; }
        public int CriticalStockCount { get; set; }
        public int LowStockCount { get; set; }
    }
}