using API_BigFOOD.DTOs;
using API_BigFOOD.Models;

namespace API_BigFOOD.Services
{
    //Interfaz encargada de definir la generación de facturas PDF
    public interface IPdfServices
    {
        //Método encargado de generar una factura PDF
        byte[] GenerarPdfFactura(
            Factura factura,
            Cliente cliente,
            List<DetalleFacturaPdfDTO> detalles,
            decimal tipoCambio);
    }
}