using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public interface ICartRepository
    {
        Task<CartDto> GetCartByUserIdAsync(string userId);
        Task<CartDto> CreateUpdateCart(CartDto cartDto);
        Task<bool> RemoveFromCart(int cartDetailsId);
        Task<bool> ClearCart(string userId);
    }
}
