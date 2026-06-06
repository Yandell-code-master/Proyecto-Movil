using System.ComponentModel.DataAnnotations;

namespace API_BigFOOD.Models
{
    public class Cliente
    {
        [Key]
        public string CedulaLegal { get; set; } = string.Empty;

        public string TipoCedula { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public bool Estado { get; set; }

        public int UsuarioId { get; set; }
    }
}
