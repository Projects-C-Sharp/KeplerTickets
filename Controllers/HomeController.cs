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

        // Only show active showtimes
        showtimes = showtimes
            .Where(s => s.Status == "Active" || s.Status == "Scheduled")
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
