namespace StockManager.Models
{
    public class StockMovementViewModel
    {
        public int Id { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string CategoryPath { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTime Date { get; set; }

        public string? Note { get; set; }
    }
}
