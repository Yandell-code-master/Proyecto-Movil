namespace API_BigFOOD.DTOs
{
    public class FacturaDTO
    {
        public string CedulaCliente { get; set; } = string.Empty;

        public string TipoPago { get; set; } = string.Empty;

        public string Condicion { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        //Metodo encargado de almacenar el listado de productos de la factura
        public List<DetalleFacturaDTO> Detalle { get; set; }
            = new List<DetalleFacturaDTO>();
    }
}
