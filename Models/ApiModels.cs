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

/// <summary>
/// Respuesta directa de POST /api/auth/login
/// (sin wrapper ApiResponse, solo { accessToken, refreshToken })
/// </summary>
public class LoginTokenResponse
{
    public string AccessToken  { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}

/// <summary>
/// Respuesta de GET /api/auth/me (envuelta en ApiResponse)
/// </summary>
public class UserProfileDto
{
    public string  FullName { get; set; } = "";
    public string  Email    { get; set; } = "";
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Modelo interno que combina tokens + perfil para la sesión
/// </summary>
public class LoginResult
{
    public string AccessToken  { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public string Role         { get; set; } = "";
    public string FullName     { get; set; } = "";
    public string Email        { get; set; } = "";
}

// ── Events ────────────────────────────────────────────────────────────────────
// EventType: Movie=0, Concert=1, Theater=2, Sports=3, Other=4
public class EventDto
{
    public int      Id              { get; set; }
    public string   Name            { get; set; } = "";
    public string   Description     { get; set; } = "";
    public int      Type            { get; set; }   // API devuelve int
    public string?  PosterUrl       { get; set; }   // antes ImageUrl
    public string?  VenueName       { get; set; }
    public string?  VenueCity       { get; set; }
    public int      DurationMinutes { get; set; }
    public bool     IsActive        { get; set; }
    public DateTime CreatedAt       { get; set; }

    // Helpers para las vistas
    public string? ImageUrl   => PosterUrl;         // alias de compatibilidad
    public string  TypeLabel  => Type switch
    {
        0 => "Movie",
        1 => "Concert",
        2 => "Theater",
        3 => "Sports",
        4 => "Other",
        _ => "Other"
    };
}

// ── Showtimes ─────────────────────────────────────────────────────────────────
// ShowtimeStatus: Active=0, Cancelled=1, Completed=2, SoldOut=3
public class ShowtimeDto
{
    public int      Id             { get; set; }
    public int      EventId        { get; set; }
    public string   EventName      { get; set; } = "";
    public DateTime StartTime      { get; set; }
    public DateTime EndTime        { get; set; }
    public decimal  BasePrice      { get; set; }
    public int      Status         { get; set; }   // API devuelve int
    public int      AvailableSeats { get; set; }
    public int      TotalSeats     { get; set; }

    // Helper para las vistas
    public string StatusLabel => Status switch
    {
        0 => "Active",
        1 => "Cancelled",
        2 => "Completed",
        3 => "SoldOut",
        _ => "Unknown"
    };

    /// <summary>
    /// True si la función ya ocurrió o está en curso ahora mismo.
    /// En curso = StartTime ya pasó pero EndTime aún no (o no hay EndTime conocida,
    /// usamos duración típica de 3 h como fallback).
    /// </summary>
    public bool IsPastOrOngoing
    {
        get
        {
            var now = DateTime.Now;
            // Si EndTime es válida la usamos; si no, StartTime + 3 h de gracia
            var end = EndTime > StartTime ? EndTime : StartTime.AddHours(3);
            return now >= StartTime && now < end   // en curso
                || now >= end;                      // ya terminó
        }
    }

    /// <summary>Sólo ya terminó (no en curso).</summary>
    public bool IsFinished
    {
        get
        {
            var now = DateTime.Now;
            var end = EndTime > StartTime ? EndTime : StartTime.AddHours(3);
            return now >= end;
        }
    }

    /// <summary>Función corriendo ahora mismo.</summary>
    public bool IsOngoing
    {
        get
        {
            var now = DateTime.Now;
            var end = EndTime > StartTime ? EndTime : StartTime.AddHours(3);
            return now >= StartTime && now < end;
        }
    }
}

// ── Seats ─────────────────────────────────────────────────────────────────────
// SeatStatus: Available=0, Reserved=1, Sold=2
// SeatType:   Standard=0, Premium=1,   VIP=2
public class SeatDto
{
    public int       Id            { get; set; }
    public string    Row           { get; set; } = "";
    public int       Number        { get; set; }
    public string    Label         { get; set; } = "";
    public int       Type          { get; set; }   // API devuelve int
    public int       Status        { get; set; }   // API devuelve int
    public DateTime? ReservedUntil { get; set; }

    public string StatusLabel => Status switch
    {
        0 => "Available",
        1 => "Reserved",
        2 => "Sold",
        _ => "Reserved"
    };

    public string TypeLabel => Type switch
    {
        0 => "Standard",
        1 => "Premium",
        2 => "VIP",
        _ => "Standard"
    };
}

// ── Customers ─────────────────────────────────────────────────────────────────
public class CustomerLookupDto
{
    public string  UserId   { get; set; } = "";
    public string  FullName { get; set; } = "";
    public string  Email    { get; set; } = "";
    public string? Phone    { get; set; }
    public string? PhotoUrl { get; set; }
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