using Microsoft.EntityFrameworkCore;

namespace API_Security.Models
{
    public class DbContextUsuario : DbContext
    {

        public DbContextUsuario(DbContextOptions<DbContextUsuario> options) : base(options)
        {

        }

        public DbSet<Usuario> Usuarios
        {
            get; set;
        }
    }
}
