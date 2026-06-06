namespace API_BigFOOD.Services
{
    //Interfaz encargada del envío de correos electrónicos
    public interface IEmailServices
    {
        //Método encargado de enviar un correo con un archivo adjunto
        Task EnviarCorreoConAdjuntoAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            byte[] adjunto,
            string nombreAdjunto);
    }
}