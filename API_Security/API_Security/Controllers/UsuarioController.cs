using Microsoft.AspNetCore.Mvc;
using API_Security.Models;
using API_Security.Services;


namespace API_Security.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {

        private DbContextUsuario dbContextUsario;
        private IAuthorizationService authorizationService;


        public UsuarioController(DbContextUsuario dbContextUsario, IAuthorizationService authorizationService) { 
            this.dbContextUsario = dbContextUsario;
            this.authorizationService = authorizationService;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login(Usuario usuario) {
            Usuario temp = dbContextUsario.Usuarios.Where(u => u.Email == usuario.Email && u.Password == usuario.Password).FirstOrDefault();

            if (temp == null) 
            {
                return Unauthorized(new AuthorizationResponse() { Msj = "No existe el usuario", Token = "", Result = false});
            }

            return Ok(new AuthorizationResponse() { Msj = "ok", Result = true, Token = authorizationService.DevolverToken(temp.Email) });
        }

        [HttpGet]
        [Route("list")]
        public IActionResult List()
        {

            return Ok(dbContextUsario.Usuarios.ToList());
        }
    }
}
