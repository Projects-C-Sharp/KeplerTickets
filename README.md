# Kepler Tickets — Módulo de Recepción

**ASP.NET Core 9 MVC** · `tickets.kepler.andrescortes.dev`

Módulo de venta asistida en mostrador. Solo accesible para roles **Receptionist** y **Admin**.

---

## Stack

| Capa | Tecnología |
|------|-----------|
| Framework | ASP.NET Core 9 MVC (Razor) |
| Sesión/Auth | Session + JWT almacenado en session |
| HTTP Client | `HttpClient` → `IApiService` |
| Estilos | CSS puro (paleta Calcite) |
| Deploy | Docker (Linux) |

---

## Flujo de venta (3 pasos)

```
1. Seleccionar función  →  Home/Index
2. Buscar/Registrar cliente  +  Selección visual de asientos  →  Sales/Seats
3. Confirmar pago  →  API receptionist/checkout
```

---

## Configuración

### `appsettings.json`
```json
{
  "ApiBase": "https://api.kepler.andrescortes.dev/"
}
```

### Variables de entorno (Docker)
```
ApiBase=https://api.kepler.andrescortes.dev/
ASPNETCORE_ENVIRONMENT=Production
```

---

## Correr localmente

```bash
cd KeplerTickets
dotnet run
# → http://localhost:5000
```

## Docker

```bash
docker build -t kepler-tickets .
docker run -p 8080:8080 \
  -e ApiBase=https://api.kepler.andrescortes.dev/ \
  kepler-tickets
```

---

## Endpoints de la API consumidos

| Método | Endpoint | Uso |
|--------|----------|-----|
| POST | `/api/auth/login` | Login |
| GET | `/api/events` | Listar eventos |
| GET | `/api/showtimes` | Listar funciones |
| GET | `/api/showtimes/{id}` | Detalle función |
| GET | `/api/showtimes/{id}/seats` | Mapa de asientos |
| GET | `/api/receptionist/customers/lookup?email=` | Buscar cliente |
| POST | `/api/receptionist/customers` | Registrar cliente |
| POST | `/api/receptionist/reserve` | Reservar asientos (30 min TTL) |
| POST | `/api/receptionist/checkout` | Completar venta + generar tickets |
| GET | `/api/receptionist/orders/{id}/tickets` | Tickets de una orden |
| POST | `/api/receptionist/orders/{id}/resend-email` | Reenviar tickets |

---

## Estructura del proyecto

```
KeplerTickets/
├── Controllers/
│   ├── AccountController.cs    # Login / Logout
│   ├── HomeController.cs       # Dashboard de funciones
│   └── SalesController.cs      # Flujo de venta + endpoints AJAX
├── Models/
│   ├── ApiModels.cs            # DTOs que refleja la API
│   └── ViewModels.cs           # ViewModels para Razor
├── Services/
│   ├── IApiService.cs
│   └── ApiService.cs           # HTTP client wrapper
├── Views/
│   ├── Account/Login.cshtml
│   ├── Home/Index.cshtml       # Grid de funciones
│   ├── Sales/Seats.cshtml      # Mapa de asientos + panel de venta
│   └── Shared/_Layout.cshtml
├── wwwroot/
│   ├── css/site.css            # Paleta Calcite + diseño
│   └── js/site.js
├── Program.cs
├── appsettings.json
└── Dockerfile
```
