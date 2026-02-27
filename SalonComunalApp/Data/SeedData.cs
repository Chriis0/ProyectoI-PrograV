using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalonComunalApp.Models;

namespace SalonComunalApp.Data
{
    public static class SeedData
    {
        public static async Task InicializarRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // aqu[i se crean los roles
            string[] roles = { "Administrador", "Comprador" };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // admin por defecto
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

            // Agregar productos si no existen
            if (!await context.Productos.AnyAsync())
            {
                var productos = new List<Producto>
                {
                    // Mobilario - Sillas
                    new Producto { Nombre = "Silla plástica básica", Descripcion = "Silla plástica apilable estándar, ideal para eventos con gran cantidad de invitados. Liviana y fácil de transportar.", Precio = 500, Categoria = "Mobiliario", Disponible = true, ImagenUrl = "https://www.miplastihogar.com/cdn/shop/files/5D7A7050-B821-4D0A-AB0D-04775D579D56.jpg?v=1687793440" },
                    new Producto { Nombre = "Silla Tiffany", Descripcion = "Silla elegante de resina transparente, perfecta para bodas y eventos formales. Resistente y estética.", Precio = 1500, Categoria = "Mobiliario", Disponible = true, ImagenUrl = "https://perfecteventmexico.com/wp-content/uploads/productos/SM010.jpg" },
                    new Producto { Nombre = "Silla ejecutiva con apoyabrazos", Descripcion = "Silla acolchonada con apoyabrazos, ideal para reuniones de negocios o eventos corporativos. Mayor comodidad para los invitados.", Precio = 3000, Categoria = "Mobiliario", Disponible = true, ImagenUrl = "https://www.idcmayoristas.com/wp-content/uploads/2024/10/silla-de-oficina-906-negra-008503-laptops-quito-3-1024x1024.png" },

                    // Mobilario - Mesas
                    new Producto { Nombre = "Mesa plástica rectangular básica", Descripcion = "Mesa plástica rectangular para 6 personas, liviana y fácil de transportar. Ideal para eventos informales y reuniones comunales.", Precio = 2000, Categoria = "Mobiliario", Disponible = true, ImagenUrl = "https://smartoffices.mx/wp-content/uploads/330-1.png" },
                    new Producto { Nombre = "Mesa redonda para banquetes", Descripcion = "Mesa redonda para 8 personas, perfecta para bodas, quinceaños y eventos formales. Superficie lisa y resistente.", Precio = 4500, Categoria = "Mobiliario", Disponible = true, ImagenUrl = "https://www.forbesgroup.eu/img-cache/folding-table-round-banquet-alu-lite-beech-top-black-frame-web.84aec15c.jpg" },
                    new Producto { Nombre = "Mesa de madera tipo rústico", Descripcion = "Mesa de madera de pino tratada para 10 personas. Ideal para eventos temáticos, ferias y celebraciones al aire libre.", Precio = 8000, Categoria = "Mobiliario", Disponible = true, ImagenUrl = "https://1.bp.blogspot.com/-pRwNUpMcXqA/W-oR5LKnPsI/AAAAAAABQbI/086xvUKaf_QdJc__xFYJ4zWm39vEpn2kQCLcBGAs/s1600/mesas%2Bde%2Bmadera%2Brusticas%2Bartesanales%2B9.jpg" },

                    // Alimentación
                    new Producto { Nombre = "Bocadillos variados", Descripcion = "Bandeja de bocadillos surtidos para 10 personas. Incluye emparedados, galletas saladas y pastelitos.", Precio = 8000, Categoria = "Alimentación", Disponible = true, ImagenUrl = "https://thumbs.dreamstime.com/b/tabla-de-banquete-abastecimiento-maravillosamente-adornada-con-diversos-bocados-y-aperitivos-la-comida-en-corporativo-celebraci%C3%B3n-141376576.jpg" },
                    new Producto { Nombre = "Almuerzo buffet básico", Descripcion = "Servicio de almuerzo buffet para 10 personas. Incluye arroz, frijoles, ensalada y un plato fuerte.", Precio = 15000, Categoria = "Alimentación", Disponible = true, ImagenUrl = "https://4.bp.blogspot.com/-ma7xXK_PqgI/XFette5FrRI/AAAAAAAAWVE/isCFUp9-C4Aw7YAp-mqzTLZxdml189-TgCK4BGAYYCw/s1600/foto%2Bbuffet%2Bcriollo%2B-%2Bempresas-a2.jpg" },
                    new Producto { Nombre = "Almuerzo buffet premium", Descripcion = "Servicio de almuerzo buffet completo para 10 personas. Incluye entrada, dos platos fuertes, postre y bebida.", Precio = 25000, Categoria = "Alimentación", Disponible = true, ImagenUrl = "https://i.pinimg.com/originals/1d/0c/a8/1d0ca80db447689d4528617facda88b2.jpg" },

                    // Bebidas
                    new Producto { Nombre = "Refresco natural", Descripcion = "Jarra de refresco natural de frutas tropicales para 10 personas. Preparado con frutas frescas.", Precio = 2500, Categoria = "Bebidas", Disponible = true, ImagenUrl = "https://i.pinimg.com/736x/a8/f6/49/a8f649191b4e792c276461916304f566.jpg" },
                    new Producto { Nombre = "Paquete de agua embotellada", Descripcion = "Paquete de 24 botellas de agua purificada de 500ml. Ideal para mantener hidratados a los invitados.", Precio = 3500, Categoria = "Bebidas", Disponible = true, ImagenUrl = "https://m.media-amazon.com/images/I/71ZwFeqwc0L._AC_SX679_.jpg" },
                    new Producto { Nombre = "Servicio de café y té", Descripcion = "Servicio completo de café y té para 20 personas. Incluye azúcar, cremora y tazas desechables.", Precio = 5000, Categoria = "Bebidas", Disponible = true, ImagenUrl = "https://boxlunchmexico.com.mx/assets/images/whatsapp-image-2020-11-02-at-14.11.59.jpeg" },

                    // Tecnología
                    new Producto { Nombre = "Proyector básico", Descripcion = "Proyector HD de 3000 lúmenes, ideal para presentaciones y películas. Incluye cable HDMI.", Precio = 15000, Categoria = "Tecnología", Disponible = true, ImagenUrl = "https://soundfybcn.com/wp-content/uploads/2023/06/Pack-Proyeccion-Plus-1024x726.webp" },
                    new Producto { Nombre = "Computadora portátil", Descripcion = "Laptop con Windows 11, Office instalado y conexión WiFi. Perfecta para presentaciones y videoconferencias.", Precio = 10000, Categoria = "Tecnología", Disponible = true, ImagenUrl = "https://i5.walmartimages.com.mx/mg/gm/3pp/asr/f2727a9c-68ea-4e70-b51b-29f6b4a13eeb.da23e07c062638d0f8c50258ff4bda1d.jpeg?odnHeight=2000&odnWidth=2000&odnBg=ffffff" },
                    new Producto { Nombre = "Equipo de sonido completo", Descripcion = "Sistema de sonido con dos bocinas, micrófono inalámbrico y consola de audio. Ideal para eventos con música o presentaciones.", Precio = 20000, Categoria = "Tecnología", Disponible = true, ImagenUrl = "https://buffetscriollojuly.com/wp-content/uploads/2015/03/ALQUILER-EQUIPO-DE-SONIDO.jpg" },

                    // Otro
                    new Producto { Nombre = "Mantel decorativo", Descripcion = "Mantel de tela para mesa de 6 personas. Disponible en colores blanco, azul y dorado.", Precio = 1000, Categoria = "Otro", Disponible = true, ImagenUrl = "https://m.media-amazon.com/images/I/71l9r8RSIxL._AC_SL1500_.jpg" },
                    new Producto { Nombre = "Arreglo floral de mesa", Descripcion = "Centro de mesa con flores naturales de temporada. Añade elegancia a cualquier evento.", Precio = 4000, Categoria = "Otro", Disponible = true, ImagenUrl = "https://globosfloresyfiestas.com/wp-content/uploads/2021/07/arreglo-floral-de-hortensias-pastel.jpg" },
                    new Producto { Nombre = "Paquete de decoración básica", Descripcion = "Incluye globos, listones, centros de mesa y letrero de bienvenida personalizable.", Precio = 12000, Categoria = "Otro", Disponible = true, ImagenUrl = "https://i.ytimg.com/vi/WUOPZWu-duU/maxresdefault.jpg" },
                };

                await context.Productos.AddRangeAsync(productos);
                await context.SaveChangesAsync();
            }
        }
    }
}