using Mango.Web.Models;

namespace Mango.Web.Services.IServices
{
    public interface ICartService
    {
        Task<ResponseDto> GetCartByUserIdAsync(string userId);
        Task<ResponseDto> AddToCartAsync(CartDto cartDto);
        Task<ResponseDto> UpdateCartAsync(CartDto cartDto);
        Task<ResponseDto> RemoveCartAsync(int cartId);
        Task<ResponseDto> ApplyCouponAsync(CartDto cartDto);
        Task<ResponseDto> RemoveCouponAsync(string userId);
        Task<ResponseDto> CheckoutAsync(CartHeaderDto cartHeaderDto);
        Task<ResponseDto> ClearCartAsync(string userId);
    }
}
