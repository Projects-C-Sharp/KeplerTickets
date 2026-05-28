using System.Net.Http.Headers;
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
    public async Task<LoginResult?> LoginAsync(LoginRequest request)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(request, _json),
            Encoding.UTF8, "application/json");
        var res  = await _http.PostAsync("api/auth/login", content);
        if (!res.IsSuccessStatusCode) return null;
        var body = await res.Content.ReadAsStringAsync();
        var wrap = JsonSerializer.Deserialize<ApiResponse<LoginResult>>(body, _json);
        return wrap is { Success: true } ? wrap.Data : null;
    }

    // ── Events ────────────────────────────────────────────────────────────────
    public async Task<List<EventDto>> GetEventsAsync()
    {
        SetAuth();
        var paged = await GetAsync<PagedResult<EventDto>>("api/events?page=1&pageSize=100");
        return paged?.Items ?? new();
    }

    // ── Showtimes ─────────────────────────────────────────────────────────────
    public async Task<List<ShowtimeDto>> GetShowtimesAsync(int? eventId = null)
    {
        SetAuth();
        var url   = eventId.HasValue
            ? $"api/showtimes?page=1&pageSize=50&eventId={eventId}"
            : "api/showtimes?page=1&pageSize=50";
        var paged = await GetAsync<PagedResult<ShowtimeDto>>(url);
        return paged?.Items ?? new();
    }

    public async Task<ShowtimeDto?> GetShowtimeAsync(int id) =>
        await GetAsync<ShowtimeDto>($"api/showtimes/{id}");

    public async Task<List<SeatDto>> GetShowtimeSeatsAsync(int showtimeId)
    {
        SetAuth();
        var seats = await GetAsync<List<SeatDto>>($"api/showtimes/{showtimeId}/seats");
        return seats ?? new();
    }

    // ── Customers ─────────────────────────────────────────────────────────────
    public async Task<CustomerLookupDto?> LookupCustomerAsync(string email)
    {
        SetAuth();
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
