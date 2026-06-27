using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FacturacionApp.Models
{
    public class FacturacionContext : IdentityDbContext<Usuario>
    {
        public FacturacionContext(DbContextOptions<FacturacionContext> options)
            : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<AuditoriaFactura> AuditoriasFactura { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DetalleFactura>()
                .Ignore(d => d.Subtotal)
                .Ignore(d => d.MontoItbis)
                .Ignore(d => d.TotalLinea);

            modelBuilder.Entity<Factura>()
                .Property(f => f.NumeroFactura)
                .IsRequired();
        }
    }
}