using System.Security.Claims;
using MedicalAppointment.API.DTOs.Cart;
using MedicalAppointment.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalAppointment.API.Controllers;

[Route("api/cart")]
[ApiController]
[Authorize]
public class CartController(ICartService cartService) : ControllerBase
{
    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }
        return null;
    }

    [HttpPost]
    public async Task<IActionResult> AddCartItem(AddCartItemRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await cartService.AddToCartAsync(userId.Value, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await cartService.GetCartAsync(userId.Value);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCartItem(int id, UpdateCartItemRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await cartService.UpdateCartItemAsync(userId.Value, id, request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCartItem(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        await cartService.DeleteCartItemAsync(userId.Value, id);
        return NoContent();
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCartCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await cartService.GetCartCountAsync(userId.Value);
        return Ok(result);
    }
}
