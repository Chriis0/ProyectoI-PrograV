using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SalonComunalApp.Controllers
{

    [Authorize(Roles = "Comprador")]
    public class CarritoController : Controller
    {
        // TODO: Inyectar ApplicationDbContext y servicios necesarios

        // GET: Ver carrito actual
        public IActionResult Index()
        {
            // TODO: Obtener productos en el carrito de la sesión
            return View();
        }

        // POST: Agregar producto al carrito
        [HttpPost]
        public IActionResult AgregarProducto(int productoId, int cantidad)
        {
            // TODO: Agregar producto al carrito en sesión
            return RedirectToAction(nameof(Index));
        }

        // POST: Eliminar producto del carrito
        [HttpPost]
        public IActionResult EliminarProducto(int productoId)
        {
            // TODO: Eliminar producto del carrito en sesión
            return RedirectToAction(nameof(Index));
        }

        // GET: Pantalla de pago
        public IActionResult Pagar()
        {
            // TODO: Mostrar resumen de compra y formulario de pago
            return View();
        }

        // POST: Procesar pago con Stripe
        [HttpPost]
        public IActionResult ProcesarPago(string stripeToken)
        {
            // TODO: Procesar pago con Stripe
            // TODO: Guardar pedido en base de datos
            // TODO: Enviar correo con fecha, productos y total
            // TODO: Vaciar carrito
            return RedirectToAction(nameof(Confirmacion));
        }

        // GET: Confirmación de compra
        public IActionResult Confirmacion()
        {
            // TODO: Mostrar resumen de la compra realizada
            return View();
        }

        // GET: Ver detalle de un producto antes de comprar
        public IActionResult DetalleProducto(int id)
        {
            // TODO: Mostrar imagen, descripción y precio del producto
            return View();
        }
    }
}