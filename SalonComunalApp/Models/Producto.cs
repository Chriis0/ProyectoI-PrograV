using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalonComunalApp.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        [Display(Name = "Nombre del producto/servicio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio (₡)")]
        public decimal Precio { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = string.Empty;

        public string? ImagenUrl { get; set; }

        [Display(Name = "Disponible")]
        public bool Disponible { get; set; } = true;

        public ICollection<DetalleReserva> DetallesReserva { get; set; } = new List<DetalleReserva>();
    }
}