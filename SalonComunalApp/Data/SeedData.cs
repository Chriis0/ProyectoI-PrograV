using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace SalonComunalApp.Data
{
    public static class SeedData
    {
        public static async Task InicializarRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // aqu[i se crean roles si no existen
            string[] roles = { "Administrador", "Comprador" };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // usuario de admin por defecto
            string emailAdmin = "admin@salon.com";
            string passwordAdmin = "Admin123*";

            var adminExiste = await userManager.FindByEmailAsync(emailAdmin);
            if (adminExiste == null)
            {
                var admin = new IdentityUser
                {
                    UserName = emailAdmin,
                    Email = emailAdmin,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(admin, passwordAdmin);
                if (resultado.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrador");
                }
            }
        }
    }
}