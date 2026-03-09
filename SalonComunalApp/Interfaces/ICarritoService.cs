using SalonComunalApp.Models;

namespace SalonComunalApp.Interfaces
{
    public interface ICarritoService
    {
        List<CarritoItem> ObtenerCarrito();
        void AgregarProducto(Producto producto, int cantidad);
        void EliminarProducto(int productoId);
        void VaciarCarrito();
        decimal ObtenerTotal();
    }
}
