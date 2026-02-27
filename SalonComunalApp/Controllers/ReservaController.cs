using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SalonComunalApp.Controllers
{
    [Authorize(Roles = "Comprador")]
    public class ReservaController : Controller
    {
        // TODO: Inyectar ApplicationDbContext y servicios necesarios

        // GET: Lista de reservas del usuario
        public IActionResult Index()
        {
            // TODO: Obtener reservas del usuario logueado
            return View();
        }

        // GET: Formulario nueva reserva
        public IActionResult Crear()
        {
            // TODO: Mostrar formulario con fecha y productos disponibles
            return View();
        }

        // POST: Guardar nueva reserva con 25% adelantado
        [HttpPost]
        public IActionResult Crear(int mesaId, DateTime fechaEvento)
        {
            // TODO: Calcular 25% del total
            // TODO: Procesar pago del adelanto con Stripe
            // TODO: Enviar correo de confirmación
            // TODO: Guardar reserva con estado "Reservado"
            return RedirectToAction(nameof(Index));
        }

        // GET: Pagar el 75% restante
        public IActionResult PagarRestante(int id)
        {
            // TODO: Verificar que falte una semana para el evento
            // TODO: Mostrar monto restante
            return View();
        }

        // POST: Procesar pago del 75% restante
        [HttpPost]
        public IActionResult PagarRestante(int id, string stripeToken)
        {
            // TODO: Procesar pago con Stripe
            // TODO: Actualizar estado a "PagadoTotal"
            // TODO: Enviar correo de confirmación de pago total
            return RedirectToAction(nameof(Index));
        }

        // GET: Cancelar reserva
        public IActionResult Cancelar(int id)
        {
            // TODO: Verificar que la cancelación sea con al menos una semana de anticipación
            // TODO: Si no cancela a tiempo, perder el adelanto y liberar el espacio
            // TODO: Enviar correo de cancelación
            return View();
        }

        // POST: Confirmar cancelación
        [HttpPost]
        public IActionResult CancelarConfirmado(int id)
        {
            // TODO: Actualizar estado a "Cancelado" o "EspacioLiberado"
            // TODO: Enviar correo respectivo
            return RedirectToAction(nameof(Index));
        }
    }
}