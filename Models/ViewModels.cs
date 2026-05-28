using System.ComponentModel.DataAnnotations;

namespace KeplerTickets.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido")]
    public string Email    { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = "";

    public string? ErrorMessage { get; set; }
}

public class ShowtimeSelectionViewModel
{
    public List<ShowtimeDto> Showtimes  { get; set; } = new();
    public List<EventDto>    Events     { get; set; } = new();
    public int?              EventFilter { get; set; }
}

public class SeatSelectionViewModel
{
    public ShowtimeDto          Showtime       { get; set; } = new();
    public List<SeatDto>        Seats          { get; set; } = new();
    public CustomerLookupDto?   Customer       { get; set; }
    public string               CustomerUserId { get; set; } = "";
}

public class ConfirmationViewModel
{
    public AssistedSaleResultDto Sale      { get; set; } = new();
    public int                   OrderId   { get; set; }
}
