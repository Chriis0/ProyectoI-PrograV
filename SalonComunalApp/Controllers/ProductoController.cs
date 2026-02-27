using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalonComunalApp.Interfaces;
using SalonComunalApp.Models;
//Christian Peña
namespace SalonComunalApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;
        public ProductoController(IProductoService productoService)
        {
            _productoService = productoService;
        }
        public async Task<IActionResult> Index()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            return View(productos);
        }
        public async Task<IActionResult> Details(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();
            return View(producto);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                await _productoService.CrearAsync(producto);
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _productoService.ActualizarAsync(producto);
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();
            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productoService.EliminarAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}