using System.ComponentModel.DataAnnotations;

namespace API_BigFOOD.Models
{
    public class Bitacora
    {
        [Key]
        public int IdBitacora { get; set; }

        public string Tabla { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        public string Maquina { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string TipoMov { get; set; } = string.Empty;

        public string Registro { get; set; } = string.Empty;
    }
}
