using SalonComunalApp.Models;

namespace SalonComunalApp.Interfaces
{

    public interface IProductoService
    {
  
        Task<IEnumerable<Producto>> ObtenerTodosAsync();

        Task<Producto?> ObtenerPorIdAsync(int id);

        Task CrearAsync(Producto producto);

        Task ActualizarAsync(Producto producto);

        Task EliminarAsync(int id);

        bool Existe(int id);
    }
}