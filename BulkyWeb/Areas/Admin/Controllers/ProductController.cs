using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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

        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            ViewData["NameDescSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["NameAscSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_asc" : "";
            ViewData["PriceDescSortParam"] = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";
            ViewData["PriceAscSortParam"] = String.IsNullOrEmpty(sortOrder) ? "price_asc" : "";

            ViewData["CurrentSort"] = sortOrder;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            IQueryable<Product> objProductList = _productRepo.GetAll(includeProperties: "Category");

            if (!String.IsNullOrEmpty(searchString))
            {
                objProductList = objProductList
                    .Where(s => s.Title.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    objProductList = objProductList.OrderByDescending(s => s.Title);
                    break;
                case "name_asc":
                    objProductList = objProductList.OrderBy(s => s.Title);
                    break;
                case "price_desc":
                    objProductList = objProductList.OrderByDescending(s => s.Price);
                    break;
                case "price_asc":
                    objProductList = objProductList.OrderBy(s => s.Price);
                    break;
                default:
                    objProductList = objProductList.OrderBy(s => s.Title);
                    break;
            }

            int pageSize = 6;

            return View(await PaginatedList<Product>.CreateAsync(objProductList.AsNoTracking(), pageNumber ?? 1, pageSize));
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
                if (file != null)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
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
                    if (string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        productVM.Product.ImageUrl = @"\images\product\book.png";
                    }

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
