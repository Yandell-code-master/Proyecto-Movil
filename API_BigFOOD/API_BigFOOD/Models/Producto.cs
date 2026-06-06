using System.ComponentModel.DataAnnotations;

namespace API_BigFOOD.Models
{
    public class Producto
    {
       
        [Key]
        // El valor es generado automáticamente por SQL Server mediante IDENTITY(1,1).
        public int CodigoInterno { get; set; } // En los métodos Save se envia en 0 porque la base de datos asigna el valor.

        public string CodigoBarra { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal PrecioVenta { get; set; }

        public decimal Descuento { get; set; }

        public decimal Impuesto { get; set; }

        public string UnidadMedida { get; set; } = string.Empty;

        public decimal PrecioCompra { get; set; }

        public int UsuarioId { get; set; }

        public int Existencia { get; set; }
    }
}
