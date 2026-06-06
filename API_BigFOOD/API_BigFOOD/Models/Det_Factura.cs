namespace API_BigFOOD.Models
{
    public class Det_Factura
    {
        public int NumFactura { get; set; }

        public int CodInterno { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public decimal PorImp { get; set; }

        public decimal PorDescuento { get; set; }
    }
}
