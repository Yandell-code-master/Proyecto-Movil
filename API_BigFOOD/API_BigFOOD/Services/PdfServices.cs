using API_BigFOOD.DTOs;
using API_BigFOOD.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API_BigFOOD.Services
{
    //Servicio encargado de generar el PDF de la factura
    public class PdfServices : IPdfServices
    {
        //Configuración de licencia QuestPDF
        static PdfServices()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        //Método encargado de generar el PDF
        public byte[] GenerarPdfFactura(
            Factura factura,
            Cliente cliente,
            List<DetalleFacturaPdfDTO> detalles,
            decimal tipoCambio)
        {
            return QuestPDF.Fluent.Document.Create(documento =>
            {
                documento.Page(page =>
                {
                    //Configuración de página
                    page.Size(PageSizes.A4);
                    page.Margin(30);

                    //Encabezado
                    page.Header().Column(col =>
                    {
                        col.Item().Text("BigFOOD")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.Orange.Medium);

                        col.Item().PaddingTop(10)
                            .Text($"Factura #{factura.Numero}")
                            .FontSize(14)
                            .Bold();
                    });

                    //Contenido principal
                    page.Content().Column(col =>
                    {
                        //Datos generales de la factura
                        col.Item().Text($"Fecha: {factura.Fecha:dd/MM/yyyy}");
                        col.Item().Text($"Cliente: {cliente.NombreCompleto}");
                        col.Item().Text($"Cédula: {cliente.CedulaLegal}");
                        col.Item().Text($"Correo: {cliente.Email}");

                        col.Item().PaddingVertical(10);

                        //Tabla de productos
                        col.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columnas =>
                            {
                                columnas.RelativeColumn(3);
                                columnas.RelativeColumn(1);
                                columnas.RelativeColumn(2);
                                columnas.RelativeColumn(2);
                            });

                            //Encabezado de la tabla
                            tabla.Header(header =>
                            {
                                header.Cell().Text("Producto").Bold();
                                header.Cell().Text("Cantidad").Bold();
                                header.Cell().Text("Precio").Bold();
                                header.Cell().Text("Subtotal").Bold();
                            });

                            //Detalle de productos
                            foreach (DetalleFacturaPdfDTO item in detalles)
                            {
                                tabla.Cell().Text(item.Descripcion);
                                tabla.Cell().Text(item.Cantidad.ToString());
                                tabla.Cell().Text($"₡{item.PrecioUnitario:N2}");
                                tabla.Cell().Text($"₡{item.Subtotal:N2}");
                            }
                        });

                        col.Item().PaddingVertical(10);

                        //Se calcula el total en dólares
                        decimal totalDolares =
                            factura.Total / tipoCambio;

                        //Totales de la factura
                        col.Item().Text($"Subtotal: ₡{factura.Subtotal:N2}");
                        col.Item().Text($"Descuento: ₡{factura.MontoDescuento:N2}");
                        col.Item().Text($"Impuesto: ₡{factura.MontoImpuesto:N2}");
                        col.Item().Text($"Total: ₡{factura.Total:N2}");

                        col.Item().PaddingTop(5);

                        col.Item().Text($"Tipo de Cambio: {tipoCambio:N2}");
                        col.Item().Text($"Total USD: ${totalDolares:N2}");
                    });

                    //Pie de página
                    page.Footer()
                        .AlignCenter()
                        .Text("Gracias por comprar en BigFOOD");
                });
            }).GeneratePdf();
        }
    }
}