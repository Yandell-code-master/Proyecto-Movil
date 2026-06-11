using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API_Security.Models;
using API_Security.Services;

namespace API_Security.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {
        private DbContextUsuario _db;
        private ITokenService _authService;

        public UsuarioController(DbContextUsuario db, ITokenService authService)
        {
            _db = db;
            _authService = authService;
        }

        // ──────────────────────────────────────────────────────
        // PÚBLICO: No requiere token
        // POST /login
        // ──────────────────────────────────────────────────────
        [HttpPost]
        [Route("/login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Usuario temp = _db.Usuarios
                .Where(u => u.Email == dto.Email && u.Password == dto.Password && u.Status == true)
                .FirstOrDefault();

            if (temp == null)
                return Unauthorized(new { Msj = "Credenciales incorrectas o usuario inactivo", Result = false });

            return Ok(new AuthorizationResponse()
            {
                Msj = "Autenticación exitosa",
                Result = true,
                Token = _authService.DevolverToken(temp.Email)
            });
        }

       
        // GET /api/usuario  — Consultar todos los usuarios
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            var lista = _db.Usuarios.ToList();
            return Ok(lista);
        }

        // GET /api/usuario/{id}  — Consultar un usuario por ID
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult GetById(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound(new { Msj = "Usuario no encontrado" });

            return Ok(usuario);
        }

        // POST /api/usuario  — Crear usuario
        [HttpPost]
        [Authorize]
        public IActionResult Create([FromBody] UsuarioDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verificar que el email no esté duplicado
            bool existe = _db.Usuarios.Any(u => u.Email == dto.Email);
            if (existe)
                return Conflict(new { Msj = "Ya existe un usuario con ese email" });

            var nuevo = new Usuario
            {
                Email = dto.Email,
                Password = dto.Password,
                Roll = dto.Roll,
                Status = dto.Status,
                Date = DateTime.Now
            };

            _db.Usuarios.Add(nuevo);
            _db.SaveChanges();

            return Ok(new { Msj = "Usuario creado correctamente", Result = true, Data = nuevo });
        }

        // PUT /api/usuario/{id}  — Actualizar usuario
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] UsuarioDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound(new { Msj = "Usuario no encontrado" });

            // Verificar email duplicado en otro usuario
            bool emailOcupado = _db.Usuarios.Any(u => u.Email == dto.Email && u.Id != id);
            if (emailOcupado)
                return Conflict(new { Msj = "Ese email ya está en uso por otro usuario" });

            usuario.Email = dto.Email;
            usuario.Password = dto.Password;
            usuario.Roll = dto.Roll;
            usuario.Status = dto.Status;

            _db.Usuarios.Update(usuario);
            _db.SaveChanges();

            return Ok(new { Msj = "Usuario actualizado correctamente", Result = true });
        }

        // DELETE /api/usuario/{id}  — Eliminar usuario
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var usuario = _db.Usuarios.Find(id);
            if (usuario == null)
                return NotFound(new { Msj = "Usuario no encontrado" });

            _db.Usuarios.Remove(usuario);
            _db.SaveChanges();

            return Ok(new { Msj = "Usuario eliminado correctamente", Result = true });
        }
    }
}
