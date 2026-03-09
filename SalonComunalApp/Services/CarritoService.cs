using SalonComunalApp.Interfaces;
using SalonComunalApp.Models;
using System.Text.Json;

namespace SalonComunalApp.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CarritoKey = "Carrito";

        public CarritoService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CarritoItem> ObtenerCarrito()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var json = session?.GetString(CarritoKey);
            if (string.IsNullOrEmpty(json))
                return new List<CarritoItem>();
            return JsonSerializer.Deserialize<List<CarritoItem>>(json) ?? new List<CarritoItem>();
        }

        public void AgregarProducto(Producto producto, int cantidad)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.ProductoId == producto.Id);
            if (item != null)
            {
                item.Cantidad += cantidad;
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Cantidad = cantidad,
                    ImagenUrl = producto.ImagenUrl
                });
            }
            GuardarCarrito(carrito);
        }

        public void EliminarProducto(int productoId)
        {
            var carrito = ObtenerCarrito();
            carrito.RemoveAll(c => c.ProductoId == productoId);
            GuardarCarrito(carrito);
        }

        public void VaciarCarrito()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(CarritoKey);
        }

        public decimal ObtenerTotal()
        {
            return ObtenerCarrito().Sum(c => c.Subtotal);
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            var json = JsonSerializer.Serialize(carrito);
            _httpContextAccessor.HttpContext?.Session.SetString(CarritoKey, json);
        }
    }
}
