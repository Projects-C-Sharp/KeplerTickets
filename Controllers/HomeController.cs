using KeplerTickets.Models;
using KeplerTickets.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeplerTickets.Controllers;

public class HomeController : Controller
{
    private readonly IApiService _api;

    public HomeController(IApiService api) => _api = api;

    public async Task<IActionResult> Index(int? eventId = null)
    {
        if (HttpContext.Session.GetString("AccessToken") == null)
            return RedirectToAction("Login", "Account");

        var showtimes = await _api.GetShowtimesAsync(eventId);
        var events    = await _api.GetEventsAsync();

        // Mostrar: activas (0), agotadas (3) y completadas (2).
        // Excluir solo Cancelled=1.
        // Ordenar: primero las futuras (asc), luego las pasadas/en curso al final.
        showtimes = showtimes
            .Where(s => s.Status == 0 || s.Status == 2 || s.Status == 3)
            .OrderBy(s => s.IsPastOrOngoing)   // false (futuras) primero
            .ThenBy(s => s.StartTime)
            .ToList();

        return View(new ShowtimeSelectionViewModel
        {
            Showtimes   = showtimes,
            Events      = events,
            EventFilter = eventId
        });
    }
}