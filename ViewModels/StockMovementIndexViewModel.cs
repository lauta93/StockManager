namespace StockManager.ViewModels
{
    public class StockMovementIndexViewModel
    {
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }

        public List<StockMovementViewModel> Movements { get; set; } = new();

        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
