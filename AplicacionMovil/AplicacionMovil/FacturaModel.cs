using System;
using System.Collections.Generic;
using System.Text;

namespace AplicacionMovil
{
    public class FacturaModel
    {
        public int Numero { get; set; }
        public DateTime Fecha { get; set; }
        public string CedulaCliente { get; set; }
        public decimal Subtotal { get; set; }
        public decimal MontoDescuento { get; set; }
        public decimal MontoImpuesto { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public int UsuarioId { get; set; }
        public string TipoPago { get; set; }
        public string Condicion { get; set; }
    }
}
