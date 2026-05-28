using KeplerTickets.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ──────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Session ──────────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite    = SameSiteMode.Strict;
    options.Cookie.Name        = "kepler.reception";
});

builder.Services.AddHttpContextAccessor();

// ── API HttpClient ────────────────────────────────────────────────────────────
var apiBase = builder.Configuration["ApiBase"]
    ?? "https://api.kepler.andrescortes.dev/";

builder.Services.AddHttpClient<IApiService, ApiService>(c =>
{
    c.BaseAddress = new Uri(apiBase.TrimEnd('/') + '/');
    c.Timeout     = TimeSpan.FromSeconds(30);
});

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
