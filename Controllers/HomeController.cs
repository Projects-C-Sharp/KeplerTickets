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

        // Solo mostrar funciones activas (Active=0) y agotadas (SoldOut=3)
        // Excluir Cancelled=1 y Completed=2
        showtimes = showtimes
            .Where(s => s.Status == 0 || s.Status == 3)
            .OrderBy(s => s.StartTime)
            .ToList();

        return View(new ShowtimeSelectionViewModel
        {
            Showtimes   = showtimes,
            Events      = events,
            EventFilter = eventId
        });
    }
}
