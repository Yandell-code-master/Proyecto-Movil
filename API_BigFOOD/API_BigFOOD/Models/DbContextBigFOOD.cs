using Microsoft.EntityFrameworkCore;

namespace API_BigFOOD.Models
{
    public class DbContextBigFOOD : DbContext
    {
        public DbContextBigFOOD(DbContextOptions<DbContextBigFOOD> options) : base(options)
        {

        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Cliente> Clientes { get; set; }

        public DbSet<Producto> Productos { get; set; }

        public DbSet<Factura> Facturas { get; set; }

        public DbSet<Det_Factura> Det_Facturas { get; set; }

        public DbSet<CuentasPorCobrar> CuentasPorCobrar { get; set; }

        public DbSet<Bitacora> Bitacora { get; set; }
        
        // Método encargado de configurar la llave primaria compuesta de DetFactura
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Det_Factura>()
                .HasKey(df => new
                {
                    df.NumFactura,
                    df.CodInterno
                });

            base.OnModelCreating(modelBuilder);
        }
    }
}