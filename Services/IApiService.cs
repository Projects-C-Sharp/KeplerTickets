using KeplerTickets.Models;

namespace KeplerTickets.Services;

public interface IApiService
{
    // Auth
    Task<LoginResult?> LoginAsync(LoginRequest request);

    // Events & Showtimes
    Task<List<EventDto>>    GetEventsAsync();
    Task<List<ShowtimeDto>> GetShowtimesAsync(int? eventId = null);
    Task<ShowtimeDto?>      GetShowtimeAsync(int id);
    Task<List<SeatDto>>     GetShowtimeSeatsAsync(int showtimeId);

    // Customers
    Task<CustomerLookupDto?> LookupCustomerAsync(string email);
    Task<CustomerLookupDto?> RegisterCustomerAsync(RegisterCustomerRequest request);

    // Receptionist flow
    Task<AssistedReserveResultDto?> ReserveSeatsAsync(AssistedReserveRequest request);
    Task<AssistedSaleResultDto?>    CheckoutAsync(AssistedCheckoutRequest request);
    Task<List<OrderTicketDto>?>     GetOrderTicketsAsync(int orderId);
    Task<ResendEmailResultDto?>     ResendEmailAsync(int orderId);
}
