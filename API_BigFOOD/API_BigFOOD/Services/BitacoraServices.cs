using API_BigFOOD.Models;

namespace API_BigFOOD.Services
{
    //Servicio encargado del registro de eventos en bitácora
    public class BitacoraServices : IBitacoraServices
    {
        //Variable para manejar la referencia del ORM
        private readonly DbContextBigFOOD dbContext;

        //Constructor con parámetros
        public BitacoraServices(DbContextBigFOOD pContext)
        {
            this.dbContext = pContext;
        }

        //Método encargado de registrar eventos en bitácora
        public async Task RegistrarEvento(
            string tabla,
            int usuarioId,
            string tipoMov,
            string registro)
        {
            Bitacora bitacora = new Bitacora
            {
                Tabla = tabla,
                UsuarioId = usuarioId,
                Maquina = Environment.MachineName,
                Fecha = DateTime.Now,
                TipoMov = tipoMov,
                Registro = registro
            };

            this.dbContext.Bitacora.Add(bitacora);

            await this.dbContext.SaveChangesAsync();
        }
    }
}