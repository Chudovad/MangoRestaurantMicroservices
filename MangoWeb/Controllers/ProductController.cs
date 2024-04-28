using Mango.Web.Models;
using Mango.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            List<ProductDto> list = new();

            var response = await _productService.GetAllProductAsync();

            if (response != null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }

            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.CreateProductAsync(model);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int productId)
        {
            var response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                ProductDto model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));

                return View(model);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.UpdateProductAsync(model);

                if (response != null && response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int productId)
        {
            var response = await _productService.GetProductByIdAsync(productId);

            if (response != null && response.IsSuccess)
            {
                ProductDto model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));

                return View(model);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                var response = await _productService.DeleteProductAsync(model.ProductId);

                if (response.IsSuccess)
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(model);
        }
    }
}
