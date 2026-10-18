using MedicalAppointment.API.DTOs.Cart;

namespace MedicalAppointment.API.Services;

public interface ICartService
{
    Task<CartItemResponse> AddToCartAsync(int userId, AddCartItemRequest request);
    Task<IEnumerable<CartItemResponse>> GetCartAsync(int userId);
    Task<CartItemResponse> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequest request);
    Task DeleteCartItemAsync(int userId, int cartItemId);
    Task<CartCountResponse> GetCartCountAsync(int userId);
}
