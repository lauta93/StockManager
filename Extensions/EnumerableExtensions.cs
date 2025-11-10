using StockManager.ViewModels;

namespace StockManager.Extensions
{
    public static class EnumerableExtensions
    {
        //Metodo que devuevle un viewmodel con los items de la pagina
        public static PaginationViewModel<T> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginationViewModel<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = source.Count()
            };
        }
    }
}