using KeplerTickets.Models;
using KeplerTickets.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeplerTickets.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _api;

    public AccountController(IApiService api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.GetString("AccessToken") != null)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _api.LoginAsync(new LoginRequest
        {
            Email    = vm.Email,
            Password = vm.Password
        });

        if (result == null)
        {
            vm.ErrorMessage = "Credenciales incorrectas. Solo recepcionistas y administradores pueden acceder.";
            return View(vm);
        }

        // Only allow Receptionist and Admin
        if (result.Role != "Receptionist" && result.Role != "Admin")
        {
            vm.ErrorMessage = "Tu rol no tiene acceso a este módulo.";
            return View(vm);
        }

        HttpContext.Session.SetString("AccessToken", result.AccessToken);
        HttpContext.Session.SetString("RefreshToken", result.RefreshToken);
        HttpContext.Session.SetString("UserRole",     result.Role);
        HttpContext.Session.SetString("UserName",     result.FullName);
        HttpContext.Session.SetString("UserEmail",    result.Email);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
