using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SalonComunalApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SalonComunalApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<DetalleReserva> DetallesReserva { get; set; }
    }
}
