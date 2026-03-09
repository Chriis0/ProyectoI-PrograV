using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalonComunalApp.Data;
using SalonComunalApp.Interfaces;
using SalonComunalApp.Models;
using Stripe;

namespace SalonComunalApp.Controllers
{
    [Authorize(Roles = "Comprador")]
    public class ReservaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICorreoService _correoService;
        private readonly IProductoService _productoService;
        private readonly IConfiguration _config;
        private readonly ILogger<ReservaController> _logger;

        public ReservaController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            ICorreoService correoService,
            IProductoService productoService,
            IConfiguration config,
            ILogger<ReservaController> logger)
        {
            _context = context;
            _userManager = userManager;
            _correoService = correoService;
            _productoService = productoService;
            _config = config;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var usuario = await _userManager.GetUserAsync(User);
            var reservas = await _context.Reservas
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(r => r.UsuarioId == usuario!.Id)
                .OrderByDescending(r => r.FechaReserva)
                .ToListAsync();

            foreach (var reserva in reservas.Where(r => r.Estado == "Reservado"))
            {
                if (DateTime.Now > reserva.FechaEvento.AddDays(-7))
                {
                    reserva.Estado = "Cancelado";
                    _logger.LogInformation("Reserva {Id} cancelada por vencimiento de plazo", reserva.Id);
                    if (!string.IsNullOrEmpty(usuario?.Email))
                        await _correoService.EnviarCorreoCancelacionAsync(usuario.Email, reserva);
                }
            }
            await _context.SaveChangesAsync();

            return View(reservas);
        }

        public async Task<IActionResult> Create()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            ViewBag.Productos = productos.Where(p => p.Disponible).ToList();
            ViewBag.StripePublicKey = _config["Stripe:PublicKey"];
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DateTime fechaEvento, List<int> productosIds, List<int> cantidades, string stripeToken)
        {
            if (fechaEvento <= DateTime.Now.AddDays(2))
            {
                TempData["Error"] = "La fecha del evento debe ser al menos 2 días en el futuro.";
                return RedirectToAction(nameof(Create));
            }

            if (productosIds == null || !productosIds.Any())
            {
                TempData["Error"] = "Debe seleccionar al menos un producto o servicio.";
                return RedirectToAction(nameof(Create));
            }

            var usuario = await _userManager.GetUserAsync(User);

            decimal total = 0;
            var itemsSeleccionados = new List<(Producto producto, int cantidad)>();
            for (int i = 0; i < productosIds.Count; i++)
            {
                var producto = await _productoService.ObtenerPorIdAsync(productosIds[i]);
                if (producto != null)
                {
                    int cant = (cantidades != null && i < cantidades.Count) ? cantidades[i] : 1;
                    total += producto.Precio * cant;
                    itemsSeleccionados.Add((producto, cant));
                }
            }

            decimal adelanto = Math.Round(total * 0.25m, 2);
            decimal restante = total - adelanto;

            try
            {
                StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

                var chargeOptions = new ChargeCreateOptions
                {
                    Amount = (long)(adelanto * 100),
                    Currency = "crc",
                    Description = $"Adelanto 25% reserva salón - {usuario?.Email}",
                    Source = stripeToken,
                    ReceiptEmail = usuario?.Email
                };

                var chargeService = new ChargeService();
                var charge = await chargeService.CreateAsync(chargeOptions);

                if (charge.Status != "succeeded")
                {
                    TempData["Error"] = "El pago no fue aprobado. Intente nuevamente.";
                    return RedirectToAction(nameof(Create));
                }

                var reserva = new Reserva
                {
                    UsuarioId = usuario?.Id ?? "",
                    FechaEvento = fechaEvento,
                    FechaReserva = DateTime.Now,
                    Total = total,
                    MontoPagadoAdelanto = adelanto,
                    MontoRestante = restante,
                    Estado = "Reservado",
                    StripePaymentId = charge.Id
                };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                foreach (var (producto, cantidad) in itemsSeleccionados)
                {
                    _context.DetallesReserva.Add(new DetalleReserva
                    {
                        ReservaId = reserva.Id,
                        ProductoId = producto.Id,
                        Cantidad = cantidad,
                        PrecioUnitario = producto.Precio
                    });
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva {Id} creada. Adelanto: {Adelanto}", reserva.Id, adelanto);

                if (!string.IsNullOrEmpty(usuario?.Email))
                    await _correoService.EnviarConfirmacionReservaAsync(usuario.Email, reserva, adelanto);

                TempData["Exito"] = $"Reserva creada exitosamente. Adelanto pagado: ₡{adelanto:N2}";
                return RedirectToAction(nameof(Index));
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error Stripe al crear reserva");
                TempData["Error"] = $"Error al procesar el pago: {ex.StripeError?.Message ?? ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        public async Task<IActionResult> PagarRestante(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            var reserva = await _context.Reservas
                .Include(r => r.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuario!.Id);

            if (reserva == null)
            {
                TempData["Error"] = "Reserva no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            if (reserva.Estado != "Reservado")
            {
                TempData["Error"] = "Esta reserva no está en estado válido para pagar.";
                return RedirectToAction(nameof(Index));
            }

            var fechaLimite = reserva.FechaEvento.AddDays(-7);
            if (DateTime.Now > fechaLimite)
            {
                TempData["Error"] = "El plazo para pagar el restante ha vencido.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StripePublicKey = _config["Stripe:PublicKey"];
            ViewBag.FechaLimite = fechaLimite.ToString("dd/MM/yyyy");
            return View(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> PagarRestante(int id, string stripeToken)
        {
            var usuario = await _userManager.GetUserAsync(User);
            var reserva = await _context.Reservas
                .Include(r => r.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuario!.Id);

            if (reserva == null || reserva.Estado != "Reservado")
            {
                TempData["Error"] = "Reserva no válida.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

                var chargeOptions = new ChargeCreateOptions
                {
                    Amount = (long)(reserva.MontoRestante * 100),
                    Currency = "crc",
                    Description = $"Pago restante 75% reserva #{reserva.Id}",
                    Source = stripeToken,
                    ReceiptEmail = usuario?.Email
                };

                var chargeService = new ChargeService();
                var charge = await chargeService.CreateAsync(chargeOptions);

                if (charge.Status != "succeeded")
                {
                    TempData["Error"] = "El pago no fue aprobado.";
                    return RedirectToAction(nameof(PagarRestante), new { id });
                }

                reserva.Estado = "PagadoTotal";
                reserva.MontoPagadoAdelanto = reserva.Total;
                reserva.MontoRestante = 0;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva {Id} pagada en su totalidad", reserva.Id);

                if (!string.IsNullOrEmpty(usuario?.Email))
                    await _correoService.EnviarConfirmacionPagoTotalAsync(usuario.Email, reserva);

                TempData["Exito"] = "Pago total procesado. ¡Su reserva está confirmada!";
                return RedirectToAction(nameof(Index));
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error Stripe al pagar restante reserva {Id}", id);
                TempData["Error"] = $"Error al procesar el pago: {ex.StripeError?.Message ?? ex.Message}";
                return RedirectToAction(nameof(PagarRestante), new { id });
            }
        }

        public async Task<IActionResult> Cancelar(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            var reserva = await _context.Reservas
                .Include(r => r.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuario!.Id);

            if (reserva == null || reserva.Estado != "Reservado")
            {
                TempData["Error"] = "Reserva no válida para cancelar.";
                return RedirectToAction(nameof(Index));
            }

            var fechaLimite = reserva.FechaEvento.AddDays(-7);
            ViewBag.PierdeAdelanto = DateTime.Now > fechaLimite;
            ViewBag.FechaLimite = fechaLimite.ToString("dd/MM/yyyy");
            return View(reserva);
        }

        [HttpPost]
        public async Task<IActionResult> CancelarConfirmado(int id)
        {
            var usuario = await _userManager.GetUserAsync(User);
            var reserva = await _context.Reservas
                .Include(r => r.Detalles).ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuario!.Id);

            if (reserva == null || reserva.Estado != "Reservado")
            {
                TempData["Error"] = "Reserva no válida.";
                return RedirectToAction(nameof(Index));
            }

            bool pierdeAdelanto = DateTime.Now > reserva.FechaEvento.AddDays(-7);
            reserva.Estado = pierdeAdelanto ? "CanceladoSinReembolso" : "Cancelado";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Reserva {Id} cancelada. PierdeAdelanto: {Pierde}", reserva.Id, pierdeAdelanto);

            if (!string.IsNullOrEmpty(usuario?.Email))
                await _correoService.EnviarCorreoCancelacionAsync(usuario.Email, reserva);

            TempData["Exito"] = pierdeAdelanto
                ? "Reserva cancelada. El adelanto no será reembolsado por cancelación tardía."
                : "Reserva cancelada exitosamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
