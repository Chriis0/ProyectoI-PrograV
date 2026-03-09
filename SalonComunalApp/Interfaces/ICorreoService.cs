using SalonComunalApp.Models;

namespace SalonComunalApp.Interfaces
{
    public interface ICorreoService
    {
        Task EnviarConfirmacionCompraAsync(string destinatario, List<CarritoItem> items, decimal total, DateTime fechaCompra);
        Task EnviarConfirmacionReservaAsync(string destinatario, Reserva reserva, decimal adelanto);
        Task EnviarConfirmacionPagoTotalAsync(string destinatario, Reserva reserva);
        Task EnviarCorreoCancelacionAsync(string destinatario, Reserva reserva);
    }
}
