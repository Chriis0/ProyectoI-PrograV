using Microsoft.EntityFrameworkCore;
using SalonComunalApp.Data;
using SalonComunalApp.Interfaces;
using SalonComunalApp.Models;
using SalonComunalApp.Services;

namespace SalonComunalApp.Services
{
    public class ProductoService : ServicioBase, IProductoService
    {
        private readonly ApplicationDbContext _context;

        public ProductoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            if (!IdEsValido(id)) return null;
            return await _context.Productos.FindAsync(id);
        }

        public async Task CrearAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
        }


        public bool Existe(int id)
        {
            return _context.Productos.Any(p => p.Id == id);
        }
    }
}
