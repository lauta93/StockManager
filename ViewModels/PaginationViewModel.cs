namespace StockManager.ViewModels
{
    public class PaginationViewModel<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
        public List<int> PageSizeOptions { get; } = new List<int> {10, 20, 50 };
    }
}
