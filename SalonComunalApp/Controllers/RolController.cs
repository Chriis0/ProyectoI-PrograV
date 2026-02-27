
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SalonComunalApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolController : Controller
    {
        // TODO: Inyectar UserManager y RoleManager
        // private readonly UserManager<IdentityUser> _userManager;
        // private readonly RoleManager<IdentityRole> _roleManager;

        // TODO: Implementacion de constructor con dependencias

        // GET: Lista de usuarios y sus roles
        public IActionResult Index()
        {
            // TODO: Obtener lista de usuarios con sus roles
            return View();
        }

        // GET: Asignar rol a usuario
        public IActionResult AsignarRol(string userId)
        {
            // TODO: Obtener usuario por ID y mostrar formulario
            return View();
        }

        // POST: Guardar rol asignado
        [HttpPost]
        public IActionResult AsignarRol(string userId, string rol)
        {
            // TODO: Guardar el rol asignado al usuario
            return RedirectToAction(nameof(Index));
        }

        // GET: Quitar rol a usuario
        public IActionResult QuitarRol(string userId)
        {
            // TODO: Quitar rol al usuario
            return RedirectToAction(nameof(Index));
        }
    }
}