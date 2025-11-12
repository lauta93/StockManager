using Microsoft.AspNetCore.Mvc;
using StockManager.Services;
using StockManager.ViewModels;

namespace StockManager.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;
        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TopSellingProducts = await _dashboardService.GetTopSellingProductsAsync(),
                LowStockProducts = await _dashboardService.GetLowStockProductsAsync(),
                Summary = await _dashboardService.GetDashboardSummaryAsync()
            };
            return View(viewModel);
        }
    }
}