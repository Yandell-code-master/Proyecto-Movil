namespace API_BigFOOD.Services
{
    //Interfaz encargada del registro de eventos en bitácora
    public interface IBitacoraServices
    {
        //Método encargado de registrar eventos en bitácora
        Task RegistrarEvento(
            string tabla,
            int usuarioId,
            string tipoMov,
            string registro);
    }
}