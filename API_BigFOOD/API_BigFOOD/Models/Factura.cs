using System.ComponentModel.DataAnnotations;

namespace API_BigFOOD.Models
{
    public class Factura
    {
        [Key]
        // El valor es generado automáticamente por SQL Server mediante IDENTITY(1,1).
        public int Numero { get; set; } // En los métodos Save se envia en 0 porque la base de datos asigna el valor.

        public DateTime Fecha { get; set; }

        public string CedulaCliente { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }

        public decimal MontoDescuento { get; set; }

        public decimal MontoImpuesto { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        public string TipoPago { get; set; } = string.Empty;

        public string Condicion { get; set; } = string.Empty;
    }
}
