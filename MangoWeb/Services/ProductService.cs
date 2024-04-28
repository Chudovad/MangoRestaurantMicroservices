using Mango.Web.Models;
using Mango.Web.Services.IServices;

namespace Mango.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly IBaseService _baseService;
        public ProductService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto> CreateProductAsync(ProductDto productDto)
        {
            return await _baseService.SendAsync(new ApiRequest()
            {
                ApiType = SD.ApiType.POST,
                Data = productDto,
                Url = SD.ProductAPIBase + "/api/products",
            });
        }

        public async Task<ResponseDto> DeleteProductAsync(int id)
        {
            return await _baseService.SendAsync(new ApiRequest()
            {
                ApiType = SD.ApiType.DELETE,
                Url = SD.ProductAPIBase + "/api/products/" + id
            });
        }

        public async Task<ResponseDto> GetAllProductAsync()
        {
            return await _baseService.SendAsync(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/products"
            });
        }



        public async Task<ResponseDto> GetProductByIdAsync(int id)
        {
            return await _baseService.SendAsync(new ApiRequest()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ProductAPIBase + "/api/products/" + id
            });
        }

        public async Task<ResponseDto> UpdateProductAsync(ProductDto productDto)
        {
            return await _baseService.SendAsync(new ApiRequest()
            {
                ApiType = SD.ApiType.PUT,
                Data = productDto,
                Url = SD.ProductAPIBase + "/api/products",
            });
        }
    }
}
