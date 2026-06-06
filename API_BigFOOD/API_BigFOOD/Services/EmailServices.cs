using System.Net;
using System.Net.Mail;

namespace API_BigFOOD.Services
{
    public class EmailServices : IEmailServices
    {

        //Variable para acceder a la configuración del proyecto
        private readonly IConfiguration configuration;

        //Variable para registrar eventos y errores
        private readonly ILogger<EmailServices> logger;

        //Constructor con parámetros
        public EmailServices(
            IConfiguration pConfiguration,
            ILogger<EmailServices> pLogger)
        {
            this.configuration = pConfiguration;
            this.logger = pLogger;
        }

        //Método encargado de enviar un correo con un archivo adjunto
        public async Task EnviarCorreoConAdjuntoAsync(
            string destinatario,
            string asunto,
            string cuerpo,
            byte[] adjunto,
            string nombreAdjunto)
        {
            //Se obtienen los datos de configuración SMTP
            string host =
                this.configuration["EmailSettings:Host"] ?? "";

            int port =
                this.configuration.GetValue<int>("EmailSettings:Port", 587);

            string username =
                this.configuration["EmailSettings:Username"] ?? "";

            string password =
                this.configuration["EmailSettings:Password"] ?? "";

            string fromName =
                this.configuration["EmailSettings:FromName"] ?? "BIGFOOD";

            string fromEmail =
                this.configuration["EmailSettings:FromEmail"] ?? "";

            bool enableSsl =
                this.configuration.GetValue<bool>("EmailSettings:EnableSsl", true);

            //Se valida si existe configuración SMTP
            if (string.IsNullOrEmpty(host))
            {
                this.logger.LogWarning(
                    "No existe configuración SMTP para envío de correos.");

                return;
            }

            try
            {
                //Se configura el cliente SMTP
                using var smtp = new SmtpClient(host, port);

                smtp.EnableSsl = enableSsl;

                smtp.Credentials =
                    new NetworkCredential(username, password);

                smtp.DeliveryMethod =
                    SmtpDeliveryMethod.Network;

                smtp.Timeout = 30000;

                //Se crea el mensaje
                using var mensaje = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),

                    Subject = asunto,

                    Body = cuerpo,

                    IsBodyHtml = true,

                    Priority = MailPriority.Normal
                };

                //Se agrega el destinatario
                mensaje.To.Add(destinatario);

                //Se crea el archivo adjunto
                var ms = new MemoryStream(adjunto);

                var attachment =
                    new Attachment(
                        ms,
                        nombreAdjunto,
                        "application/pdf");

                mensaje.Attachments.Add(attachment);

                //Se envía el correo
                await smtp.SendMailAsync(mensaje);

                //Se registra el envío exitoso
                this.logger.LogInformation(
                    $"Correo enviado correctamente a {destinatario}");
            }
            catch (Exception ex)
            {
                //Se registra el error
                this.logger.LogError(
                    ex,
                    $"Error al enviar correo a {destinatario}");
            }
        }
    }
}
