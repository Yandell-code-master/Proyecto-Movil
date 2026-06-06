using API_BigFOOD.DTOs;
using API_BigFOOD.Models;

//Librerías para manejar la seguridad JWT
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_BigFOOD.Services
{
    public class AuthorizationServices : IAuthorizationServices
    {
        //Variable para manejar la referencia con el servidor de base de datos
        private readonly DbContextBigFOOD _context = null;

        //Variable para utilizar el archivo appsettings.json
        private readonly IConfiguration _configuration = null;

        //Constructor con parámetros para referenciar las dependencias del ORM y del appsettings.json
        public AuthorizationServices(
            IConfiguration pConfiguration,
            DbContextBigFOOD pContext)
        {
            this._configuration = pConfiguration;
            this._context = pContext;
        }

        //Método encargado de validar las credenciales del usuario y generar el token JWT
        public AuthorizationResponse DevolverToken(LoginDTO authorization)
        {
            //Se verifica las credenciales del usuario
            Usuario temp = this._context.Usuarios.FirstOrDefault(u =>
                u.Login.Equals(authorization.Login)
                && u.Password.Equals(authorization.Password));

            //Variable para almacenar el token del usuario
            string token = "";

            //Se verifica si el usuario existe
            if (temp != null)
            {
                //Se genera el token utilizando el login como identificador
                token = this.GenerarToken(authorization.Login);

                return new AuthorizationResponse
                {
                    Token = token,
                    Result = true,
                    Msj = "Ok"
                };
            }
            else
            {
                return new AuthorizationResponse
                {
                    Token = "NA",
                    Result = false,
                    Msj = "Authentication Failed..."
                };
            }
        }

        // Método encargado de generar el token JWT
        private string GenerarToken(string login)
        {
            //Variable para almacenar el valor del token
            string tokenCreado = "";

            //Se realiza la lectura del valor almacenado en la key
            var key = this._configuration.GetValue<string>("JwtSettings:Key");

            //Se toma la llave y se convierte en bytes
            var keyBytes = Encoding.ASCII.GetBytes(key);

            //Se crea la instancia de la identidad que realiza el reclamo
            var claims = new ClaimsIdentity();

            //Se agrega el identificador en la identidad
            claims.AddClaim(
                new Claim(
                    ClaimTypes.NameIdentifier,
                    login));

            //Se definen las credenciales del token
            var credencialesToken = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256);

            //Se realiza la creación del descriptor del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims, //Se agrega la identidad
                Expires = DateTime.UtcNow.AddMinutes(5), //Duración del token
                SigningCredentials = credencialesToken //Credenciales del token
            };

            //Se instancia el TokenHandler para configurar el token
            var tokenHandler = new JwtSecurityTokenHandler();

            //Se crea el token
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            //Se escribe el token
            tokenCreado = tokenHandler.WriteToken(tokenConfig);

            //Se retorna el token
            return tokenCreado;
        }
    }//Cierre class
}//Cierre namespace