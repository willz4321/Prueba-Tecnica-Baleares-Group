using NetCoreBackend.Models;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace app_baleares.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Contacts> Contacts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de User
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Nombre)
                      .HasMaxLength(100);

                entity.Property(u => u.Correo)
                      .HasMaxLength(100);

                entity.Property(u => u.Password)
                      .HasMaxLength(300);

                entity.Property(u => u.Avatar)
                      .HasMaxLength(100);
            });

            modelBuilder.Entity<Contacts>(entity =>
            {

                entity.Property(c => c.Nombre)
                      .HasMaxLength(150);

                entity.Property(c => c.Empresa)
                      .HasMaxLength(150);

                entity.Property(c => c.Email)
                      .HasMaxLength(150);

                entity.Property(c => c.Telefono)
                  .HasMaxLength(30);

                entity.OwnsOne(c => c.Direccion, address =>
                {
                    address.Property(a => a.Calle)
                           .HasMaxLength(150);

                    address.Property(a => a.Localidad)
                           .HasMaxLength(100);
                });
            });
        }
    }
}
