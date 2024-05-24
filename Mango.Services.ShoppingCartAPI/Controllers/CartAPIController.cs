using Azure;
using Mango.Services.ShoppingCartAPI.Messages;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.RabbitMQSender;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartAPIController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IRabbitMQCartMessageSender _rabbitMQSender;
        protected ResponseDto _responseDto;

        public CartAPIController(ICartRepository cartRepository, IRabbitMQCartMessageSender rabbitMQCartMessageSender, ICouponRepository couponRepository)
        {
            _cartRepository = cartRepository;
            _responseDto = new ResponseDto();
            _rabbitMQSender = rabbitMQCartMessageSender;
            _couponRepository = couponRepository;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<object> GetCart(string userId)
        {
            try
            {
                CartDto cartDto = await _cartRepository.GetCartByUserIdAsync(userId);
                _responseDto.Result = cartDto;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }

        [HttpPost("AddCart")]
        public async Task<object> AddCart(CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _responseDto.Result = cartDt;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }

        [HttpPost("UpdateCart")]
        public async Task<object> UpdateCart(CartDto cartDto)
        {
            try
            {
                CartDto cartDt = await _cartRepository.CreateUpdateCart(cartDto);
                _responseDto.Result = cartDt;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }

        [HttpPost("RemoveCart")]
        public async Task<object> RemoveCart([FromBody] int cartId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveFromCart(cartId);
                _responseDto.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }

        [HttpGet("ClearCart/{userId}")]
        public async Task<object> ClearCart(string userId)
        {
            try
            {
                _responseDto.IsSuccess = await _cartRepository.ClearCart(userId);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                bool isSuccess = await _cartRepository.ApplyCoupon(cartDto.CartHeader.UserId, cartDto.CartHeader.CouponCode);
                _responseDto.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] string userId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveCoupon(userId);
                _responseDto.Result = isSuccess;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }

        [HttpPost("Checkout")]
        public async Task<object> Checkout(CheckoutHeaderDto checkOutHeaderDto)
        {
            try
            {
                CartDto cartDto = await _cartRepository.GetCartByUserIdAsync(checkOutHeaderDto.UserId);

                if (cartDto == null)
                {
                    return BadRequest();
                }

                if (!string.IsNullOrEmpty(checkOutHeaderDto.CouponCode))
                {
                    CouponDto couponDto = await _couponRepository.GetCouponAsync(checkOutHeaderDto.CouponCode);
                    if (checkOutHeaderDto.DiscountTotal != couponDto.DiscountAmount)
                    {
                        _responseDto.IsSuccess = false;
                        _responseDto.ErrorMessages = new List<string>() { "Coupon Price has changed, please confirm" };
                        _responseDto.DisplayMessage = "Coupon Price has changed, please confirm";
                        return _responseDto;
                    }
                }

                checkOutHeaderDto.CartDetails = cartDto.CartDetails;
                _rabbitMQSender.SendMessage(checkOutHeaderDto, "CheckoutQueue");
                await _cartRepository.ClearCart(cartDto.CartHeader.UserId);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _responseDto;
        }
    }
}
