using System.ComponentModel.DataAnnotations;

namespace API_BigFOOD.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        public string Login { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public bool Estado { get; set; }
    }
}
