using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepo;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepo)
        {
            _logger = logger;
            _productRepo = productRepo;
        }

        public IActionResult Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParm"] = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";

            ViewData["CurrentFilter"] = searchString;

            List<Product> objProductList = _productRepo.GetAll(includeProperties: "Category").ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                objProductList = objProductList
                    .Where(s => s.Title.Contains(searchString))
                .ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    objProductList = objProductList.OrderByDescending(s => s.Title).ToList();
                    break;
                case "price_desc":
                    objProductList = objProductList.OrderByDescending(s => s.Price).ToList();
                    break;
                default:
                    objProductList = objProductList.OrderBy(s => s.Title).ToList();
                    break;
            }

            return View(objProductList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
