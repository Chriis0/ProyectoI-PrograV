using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SalonComunalApp.Data;
using SalonComunalApp.Interfaces;
using SalonComunalApp.Models;
using Stripe;

namespace SalonComunalApp.Controllers
{
    [Authorize(Roles = "Comprador")]
    public class CarritoController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICarritoService _carritoService;
        private readonly ICorreoService _correoService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<CarritoController> _logger;

        public CarritoController(
            IProductoService productoService,
            ICarritoService carritoService,
            ICorreoService correoService,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IConfiguration config,
            ILogger<CarritoController> logger)
        {
            _productoService = productoService;
            _carritoService = carritoService;
            _correoService = correoService;
            _context = context;
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            var productosDisponibles = productos.Where(p => p.Disponible).ToList();
            ViewBag.CantidadCarrito = _carritoService.ObtenerCarrito().Sum(c => c.Cantidad);
            return View(productosDisponibles);
        }

        public async Task<IActionResult> DetalleProducto(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarProducto(int productoId, int cantidad = 1)
        {
            var producto = await _productoService.ObtenerPorIdAsync(productoId);
            if (producto == null || !producto.Disponible)
            {
                TempData["Error"] = "El producto no está disponible.";
                return RedirectToAction(nameof(Index));
            }
            if (cantidad < 1) cantidad = 1;
            _carritoService.AgregarProducto(producto, cantidad);
            TempData["Exito"] = $"'{producto.Nombre}' agregado al carrito.";
            _logger.LogInformation("Producto {ProductoId} agregado al carrito por {Usuario}", productoId, User.Identity?.Name);
            return RedirectToAction(nameof(VerCarrito));
        }

        [HttpPost]
        public IActionResult EliminarProducto(int productoId)
        {
            _carritoService.EliminarProducto(productoId);
            TempData["Exito"] = "Producto eliminado del carrito.";
            _logger.LogInformation("Producto {ProductoId} eliminado del carrito por {Usuario}", productoId, User.Identity?.Name);
            return RedirectToAction(nameof(VerCarrito));
        }

        public IActionResult VerCarrito()
        {
            var items = _carritoService.ObtenerCarrito();
            ViewBag.Total = _carritoService.ObtenerTotal();
            return View(items);
        }

        public IActionResult Pagar()
        {
            var items = _carritoService.ObtenerCarrito();
            if (!items.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Total = _carritoService.ObtenerTotal();
            ViewBag.StripePublicKey = _config["Stripe:PublicKey"];
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarPago(string stripeToken)
        {
            var items = _carritoService.ObtenerCarrito();
            if (!items.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            var total = _carritoService.ObtenerTotal();
            var usuario = await _userManager.GetUserAsync(User);

            try
            {
                StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

                var chargeOptions = new ChargeCreateOptions
                {
                    Amount = (long)(total * 100),
                    Currency = "crc",
                    Description = $"Compra Salón Comunal - {usuario?.Email}",
                    Source = stripeToken,
                    ReceiptEmail = usuario?.Email
                };

                var chargeService = new ChargeService();
                var charge = await chargeService.CreateAsync(chargeOptions);

                if (charge.Status != "succeeded")
                {
                    TempData["Error"] = "El pago no fue aprobado. Intente nuevamente.";
                    return RedirectToAction(nameof(Pagar));
                }

                var reserva = new Reserva
                {
                    UsuarioId = usuario?.Id ?? "",
                    FechaEvento = DateTime.Now,
                    FechaReserva = DateTime.Now,
                    Total = total,
                    MontoPagadoAdelanto = total,
                    MontoRestante = 0,
                    Estado = "Pagado",
                    StripePaymentId = charge.Id
                };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    _context.DetallesReserva.Add(new DetalleReserva
                    {
                        ReservaId = reserva.Id,
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio
                    });
                }
                await _context.SaveChangesAsync();

                _logger.LogInformation("Compra procesada. ReservaId={ReservaId}, Total={Total}", reserva.Id, total);

                if (!string.IsNullOrEmpty(usuario?.Email))
                    await _correoService.EnviarConfirmacionCompraAsync(usuario.Email, items, total, DateTime.Now);

                TempData["ReservaId"] = reserva.Id;
                TempData["Total"] = total.ToString("N2");
                TempData["StripeId"] = charge.Id;

                _carritoService.VaciarCarrito();
                return RedirectToAction(nameof(Confirmacion));
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error de Stripe al procesar pago");
                TempData["Error"] = $"Error al procesar el pago: {ex.StripeError?.Message ?? ex.Message}";
                return RedirectToAction(nameof(Pagar));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar pago");
                TempData["Error"] = "Ocurrió un error inesperado. Contacte al administrador.";
                return RedirectToAction(nameof(Pagar));
            }
        }

        public IActionResult Confirmacion()
        {
            return View();
        }
    }
}
