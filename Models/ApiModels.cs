namespace KeplerTickets.Models;

// ── Shared ────────────────────────────────────────────────────────────────────
public class ApiResponse<T>
{
    public bool    Success { get; set; }
    public string? Message { get; set; }
    public T?      Data    { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items      { get; set; } = new();
    public int     TotalCount { get; set; }
    public int     Page       { get; set; }
    public int     PageSize   { get; set; }
    public int     TotalPages { get; set; }
}

// ── Auth ──────────────────────────────────────────────────────────────────────
public class LoginRequest
{
    public string Email    { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResult
{
    public string AccessToken  { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string Role         { get; set; } = "";
    public string FullName     { get; set; } = "";
    public string Email        { get; set; } = "";
}

// ── Events ────────────────────────────────────────────────────────────────────
public class EventDto
{
    public int     Id          { get; set; }
    public string  Name        { get; set; } = "";
    public string  Description { get; set; } = "";
    public string  Type        { get; set; } = "";
    public string? ImageUrl    { get; set; }
    public string? VenueName   { get; set; }
}

// ── Showtimes ─────────────────────────────────────────────────────────────────
public class ShowtimeDto
{
    public int      Id             { get; set; }
    public int      EventId        { get; set; }
    public string   EventName      { get; set; } = "";
    public DateTime StartTime      { get; set; }
    public DateTime EndTime        { get; set; }
    public decimal  BasePrice      { get; set; }
    public string   Status         { get; set; } = "";
    public int      AvailableSeats { get; set; }
    public int      TotalSeats     { get; set; }
}

// ── Seats ─────────────────────────────────────────────────────────────────────
public class SeatDto
{
    public int      Id            { get; set; }
    public string   Row           { get; set; } = "";
    public int      Number        { get; set; }
    public string   Label         { get; set; } = "";
    public string   Type          { get; set; } = "";
    public string   Status        { get; set; } = "";
    public DateTime? ReservedUntil { get; set; }
}

// ── Customers ─────────────────────────────────────────────────────────────────
public class CustomerLookupDto
{
    public string  UserId   { get; set; } = "";
    public string  FullName { get; set; } = "";
    public string  Email    { get; set; } = "";
    public string? Phone    { get; set; }
    public bool    IsActive { get; set; }
    public bool    IsNew    { get; set; }
}

public class RegisterCustomerRequest
{
    public string  FullName { get; set; } = "";
    public string  Email    { get; set; } = "";
    public string? Phone    { get; set; }
}

// ── Reserve ───────────────────────────────────────────────────────────────────
public class AssistedReserveRequest
{
    public string    CustomerUserId { get; set; } = "";
    public int       ShowtimeId     { get; set; }
    public List<int> SeatIds        { get; set; } = new();
}

public class AssistedReserveResultDto
{
    public bool      Success         { get; set; }
    public string    Message         { get; set; } = "";
    public List<int> ReservedSeatIds { get; set; } = new();
    public DateTime? ExpiresAt       { get; set; }
}

// ── Checkout ──────────────────────────────────────────────────────────────────
public class AssistedCheckoutRequest
{
    public string    CustomerUserId { get; set; } = "";
    public List<int> SeatIds        { get; set; } = new();
    public string    PaymentMethod  { get; set; } = "Cash";
}

public class OrderTicketDto
{
    public int      TicketId      { get; set; }
    public string   QRCode        { get; set; } = "";
    public string?  QrImageUrl    { get; set; }
    public string   SeatLabel     { get; set; } = "";
    public string   EventName     { get; set; } = "";
    public DateTime ShowtimeStart { get; set; }
    public bool     IsUsed        { get; set; }
}

public class AssistedSaleResultDto
{
    public bool                 Success       { get; set; }
    public int                  OrderId       { get; set; }
    public string               TransactionId { get; set; } = "";
    public decimal              AmountPaid    { get; set; }
    public DateTime             PaidAt        { get; set; }
    public string               CustomerEmail { get; set; } = "";
    public string               CustomerName  { get; set; } = "";
    public string               PaymentMethod { get; set; } = "";
    public List<OrderTicketDto> Tickets       { get; set; } = new();
}

public class ResendEmailResultDto
{
    public bool   Success     { get; set; }
    public string SentTo      { get; set; } = "";
    public int    TicketsSent { get; set; }
}
