using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ProductController(IProductRepository productRepo, ICategoryRepository categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _productRepo.GetAll().ToList();

            return View(objProductList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> categoryList = _categoryRepo.GetAll()
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                });

            ProductVM productVM = new ()
            {
                CategoryList = categoryList,
                Product = new Product()
            };

            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create(ProductVM obj)
        {
            if (ModelState.IsValid)
            {
                _productRepo.Add(obj.Product);
                _productRepo.SaveChanges();
                TempData["success"] = "Product created successfully.";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Product productObj = _productRepo.Get(u => u.Id == id);

            if (productObj == null)
            {
                return NotFound();
            }

            return View(productObj);
        }

        [HttpPost]
        public IActionResult Edit(Product obj)
        {
            if (ModelState.IsValid)
            {
                _productRepo.Update(obj);
                _productRepo.SaveChanges();
                TempData["success"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }

            Product? productObj = _productRepo.Get(u => u.Id == id);

            if (productObj == null)
            {
                return NotFound();
            }

            return View(productObj);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _productRepo.Get(u => u.Id == id);

            if (obj == null)
            {
                return NotFound();
            }

            _productRepo.Remove(obj);
            _productRepo.SaveChanges();
            TempData["success"] = "Product deleted successfully.";
            return RedirectToAction("Index");

        }
    }
}
