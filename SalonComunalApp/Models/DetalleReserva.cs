using System.ComponentModel.DataAnnotations.Schema;

namespace SalonComunalApp.Models
{
    public class DetalleReserva
    {
        public int Id { get; set; }

        public int ReservaId { get; set; }
        public Reserva? Reserva { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}