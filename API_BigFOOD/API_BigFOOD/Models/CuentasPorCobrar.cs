using System.ComponentModel.DataAnnotations;

namespace API_BigFOOD.Models
{
    public class CuentasPorCobrar
    {
        [Key]
        public int NumFactura { get; set; }

        public string CedulaCliente { get; set; } = string.Empty; 

        public DateTime FechaFactura { get; set; }

        public DateTime FechaRegistro { get; set; }

        public decimal MontoFactura { get; set; }

        public int UsuarioId { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
