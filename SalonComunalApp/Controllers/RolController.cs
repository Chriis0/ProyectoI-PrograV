using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SalonComunalApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class RolController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var usuarios = _userManager.Users.ToList();
            return View(usuarios);
        }

        public async Task<IActionResult> AsignarRol(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Usuario = usuario;
            ViewBag.Roles = roles;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AsignarRol(string userId, string rol)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            var rolesActuales = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, rol);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> QuitarRol(string userId)
        {
            var usuario = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, roles);
            return RedirectToAction(nameof(Index));
        }
    }
}
