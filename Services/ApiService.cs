using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using KeplerTickets.Models;

namespace KeplerTickets.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiService(HttpClient http, IHttpContextAccessor ctx)
    {
        _http = http;
        _ctx  = ctx;
    }

    // ── Token helper ──────────────────────────────────────────────────────────
    private void SetAuth()
    {
        var token = _ctx.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Extrae el rol del claim "role" dentro del JWT sin llamar a la API.
    /// </summary>
    private static string ExtractRoleFromJwt(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt     = handler.ReadJwtToken(accessToken);

            // ASP.NET Identity usa ClaimTypes.Role que serializa como URI largo:
            // "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            // JwtSecurityTokenHandler también lo mapea al alias corto "role"
            // → buscamos ambos para cubrir cualquier variante.
            var role = jwt.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.Role ||
                    c.Type == "role" ||
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                ?.Value ?? "";

            return role;
        }
        catch
        {
            return "";
        }
    }

    private async Task<T?> GetAsync<T>(string path)
    {
        SetAuth();
        var res  = await _http.GetAsync(path);
        if (!res.IsSuccessStatusCode) return default;
        var body = await res.Content.ReadAsStringAsync();
        var wrap = JsonSerializer.Deserialize<ApiResponse<T>>(body, _json);
        return wrap is { Success: true } ? wrap.Data : default;
    }

    private async Task<T?> PostAsync<T>(string path, object payload)
    {
        SetAuth();
        var content = new StringContent(
            JsonSerializer.Serialize(payload, _json),
            Encoding.UTF8, "application/json");
        var res  = await _http.PostAsync(path, content);
        if (!res.IsSuccessStatusCode) return default;
        var body = await res.Content.ReadAsStringAsync();
        var wrap = JsonSerializer.Deserialize<ApiResponse<T>>(body, _json);
        return wrap is { Success: true } ? wrap.Data : default;
    }

    // ── Auth ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// POST /api/auth/login devuelve { accessToken, refreshToken } sin wrapper.
    /// Luego llama a GET /api/auth/me (con el token) para obtener perfil.
    /// El rol se extrae del JWT para evitar una llamada extra.
    /// </summary>
    public async Task<LoginResult?> LoginAsync(LoginRequest request)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request, _json),
            Encoding.UTF8, "application/json");

        var res = await _http.PostAsync("api/auth/login", content);
        if (!res.IsSuccessStatusCode) return null;

        var body   = await res.Content.ReadAsStringAsync();
        var tokens = JsonSerializer.Deserialize<LoginTokenResponse>(body, _json);
        if (tokens == null || string.IsNullOrEmpty(tokens.AccessToken)) return null;

        // Extraer rol del JWT (claim "role")
        var role = ExtractRoleFromJwt(tokens.AccessToken);

        // Obtener perfil del usuario autenticado
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var profileWrap = await GetAsync<UserProfileDto>("api/auth/me");

        return new LoginResult
        {
            AccessToken  = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            Role         = role,
            FullName     = profileWrap?.FullName ?? "",
            Email        = profileWrap?.Email    ?? request.Email
        };
    }

    // ── Events ────────────────────────────────────────────────────────────────
    public async Task<List<EventDto>> GetEventsAsync()
    {
        var paged = await GetAsync<PagedResult<EventDto>>("api/events?page=1&pageSize=100");
        return paged?.Items ?? new();
    }

    // ── Showtimes ─────────────────────────────────────────────────────────────
    public async Task<List<ShowtimeDto>> GetShowtimesAsync(int? eventId = null)
    {
        var url = eventId.HasValue
            ? $"api/showtimes?page=1&pageSize=50&eventId={eventId}"
            : "api/showtimes?page=1&pageSize=50";
        var paged = await GetAsync<PagedResult<ShowtimeDto>>(url);
        return paged?.Items ?? new();
    }

    public async Task<ShowtimeDto?> GetShowtimeAsync(int id) =>
        await GetAsync<ShowtimeDto>($"api/showtimes/{id}");

    public async Task<List<SeatDto>> GetShowtimeSeatsAsync(int showtimeId)
    {
        var seats = await GetAsync<List<SeatDto>>($"api/showtimes/{showtimeId}/seats");
        return seats ?? new();
    }

    // ── Customers ─────────────────────────────────────────────────────────────
    public async Task<CustomerLookupDto?> LookupCustomerAsync(string email)
    {
        var enc = Uri.EscapeDataString(email);
        return await GetAsync<CustomerLookupDto>($"api/receptionist/customers/lookup?email={enc}");
    }

    public async Task<CustomerLookupDto?> RegisterCustomerAsync(RegisterCustomerRequest req) =>
        await PostAsync<CustomerLookupDto>("api/receptionist/customers", req);

    // ── Receptionist flow ─────────────────────────────────────────────────────
    public async Task<AssistedReserveResultDto?> ReserveSeatsAsync(AssistedReserveRequest req) =>
        await PostAsync<AssistedReserveResultDto>("api/receptionist/reserve", req);

    public async Task<AssistedSaleResultDto?> CheckoutAsync(AssistedCheckoutRequest req) =>
        await PostAsync<AssistedSaleResultDto>("api/receptionist/checkout", req);

    public async Task<List<OrderTicketDto>?> GetOrderTicketsAsync(int orderId) =>
        await GetAsync<List<OrderTicketDto>>($"api/receptionist/orders/{orderId}/tickets");

    public async Task<ResendEmailResultDto?> ResendEmailAsync(int orderId)
    {
        SetAuth();
        var res  = await _http.PostAsync($"api/receptionist/orders/{orderId}/resend-email", null);
        if (!res.IsSuccessStatusCode) return null;
        var body = await res.Content.ReadAsStringAsync();
        var wrap = JsonSerializer.Deserialize<ApiResponse<ResendEmailResultDto>>(body, _json);
        return wrap?.Data;
    }
}
