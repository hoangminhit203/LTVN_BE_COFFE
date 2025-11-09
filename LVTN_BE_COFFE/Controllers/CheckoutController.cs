using LVTN_BE_COFFE.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly CheckoutService _checkoutService;

    public CheckoutController(CheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var order = await _checkoutService.CheckoutAsync(request.UserId, request.ShippingAddress, request.VoucherCode);
        return Ok(order);
    }
}
public class CheckoutRequest
{
    public string UserId { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? VoucherCode { get; set; }
}
