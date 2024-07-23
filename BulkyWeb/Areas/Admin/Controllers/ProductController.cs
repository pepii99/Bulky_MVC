using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository productRepo, ICategoryRepository categoryRepo, IWebHostEnvironment webHostEnvironment)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _productRepo.GetAll().ToList();

            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
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

            if(id == null || id == 0)
            {
                //create
                return View(productVM);
            }

            //update
            productVM.Product = _productRepo.Get(u => u.Id == id);
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (productVM.Product.ImageUrl != null)
                    {
                        //Delete old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (productVM.Product.Id == 0)
                {
                    _productRepo.Add(productVM.Product);
                }
                else
                {
                    _productRepo.Update(productVM.Product);
                }

                _productRepo.SaveChanges();
                TempData["success"] = "Product created successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _categoryRepo.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString(),
                });
            }
            return View(productVM);
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
