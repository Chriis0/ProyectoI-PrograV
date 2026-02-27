using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonComunalApp.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha del evento")]
        public DateTime FechaEvento { get; set; }

        public DateTime FechaReserva { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoPagadoAdelanto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoRestante { get; set; }

        public string Estado { get; set; } = "Reservado";

        public string? StripePaymentId { get; set; }

        public ICollection<DetalleReserva> Detalles { get; set; } = new List<DetalleReserva>();
    }
}
