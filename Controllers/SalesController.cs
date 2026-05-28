using System.Text.Json;
using KeplerTickets.Models;
using KeplerTickets.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeplerTickets.Controllers;

public class SalesController : Controller
{
    private readonly IApiService _api;

    public SalesController(IApiService api) => _api = api;

    private bool IsAuthenticated =>
        HttpContext.Session.GetString("AccessToken") != null;

    // ── Step 2: Seat Selection ────────────────────────────────────────────────
    public async Task<IActionResult> Seats(int showtimeId, string? customerId, string? customerJson)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");

        var showtime = await _api.GetShowtimeAsync(showtimeId);
        if (showtime == null) return NotFound();

        var seats = await _api.GetShowtimeSeatsAsync(showtimeId);

        CustomerLookupDto? customer = null;
        if (!string.IsNullOrEmpty(customerJson))
        {
            try { customer = JsonSerializer.Deserialize<CustomerLookupDto>(customerJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }); }
            catch { }
        }

        return View(new SeatSelectionViewModel
        {
            Showtime       = showtime,
            Seats          = seats,
            Customer       = customer,
            CustomerUserId = customerId ?? ""
        });
    }

    // ── API-style actions called via AJAX ─────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> LookupCustomer(string email)
    {
        if (!IsAuthenticated) return Unauthorized();
        var result = await _api.LookupCustomerAsync(email);
        if (result == null) return NotFound(new { message = "Cliente no encontrado" });
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequest request)
    {
        if (!IsAuthenticated) return Unauthorized();
        var result = await _api.RegisterCustomerAsync(request);
        if (result == null) return BadRequest(new { message = "Error al registrar el cliente" });
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Reserve([FromBody] AssistedReserveRequest request)
    {
        if (!IsAuthenticated) return Unauthorized();
        var result = await _api.ReserveSeatsAsync(request);
        if (result == null || !result.Success)
            return BadRequest(new { message = result?.Message ?? "Error al reservar asientos" });
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] AssistedCheckoutRequest request)
    {
        if (!IsAuthenticated) return Unauthorized();
        var result = await _api.CheckoutAsync(request);
        if (result == null)
            return BadRequest(new { message = "Error al procesar la venta" });
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> ResendEmail(int orderId)
    {
        if (!IsAuthenticated) return Unauthorized();
        var result = await _api.ResendEmailAsync(orderId);
        if (result == null)
            return BadRequest(new { message = "Error al reenviar correo" });
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetSeats(int showtimeId)
    {
        if (!IsAuthenticated) return Unauthorized();
        var seats = await _api.GetShowtimeSeatsAsync(showtimeId);
        return Json(seats);
    }

    public IActionResult Confirmation(int orderId)
    {
        if (!IsAuthenticated) return RedirectToAction("Login", "Account");
        return View(orderId);
    }
}
