using API_BigFOOD.DTOs;
using API_BigFOOD.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_BigFOOD.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        //Variable para usar el servicio JWT
        private readonly IAuthorizationServices authorizationServices = null;

        //Constructor con parámetros recibe el servicio JWT
        public AuthController(IAuthorizationServices pServices)
        {
            authorizationServices = pServices;
        }

        [HttpPost]
        [Route("AuthenticationUsers")]
        public IActionResult AuthenticationUsers(LoginDTO temp)
        {
            //Se realiza la validación de la autorización
            var authorization =
                this.authorizationServices.DevolverToken(temp);

            //Se valida si hay datos
            if (authorization.Result == false)
            {
                return Unauthorized(authorization);
            }
            else
            {
                //Se envía la información del token de autorización
                return Ok(authorization);
            }
        }
    }
}
