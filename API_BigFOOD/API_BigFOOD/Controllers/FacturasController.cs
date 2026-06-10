using API_BigFOOD.DTOs;
using API_BigFOOD.Models;
using API_BigFOOD.Services;
using Microsoft.AspNetCore.Mvc;

namespace API_BigFOOD.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FacturasController : ControllerBase
    {
        //Variable para manejar la referencia del ORM
        private readonly DbContextBigFOOD dbContext = null;
        //Variable para manejar los servicios de gometa
        private readonly GometaServices gometaServices;
        //Variable para manejar el pdfServices
        private readonly IPdfServices pdfServices;
        //Variable para manejar en emailServices
        private readonly IEmailServices emailServices;
        //Variable para manejar los eventos de la bitacora
        private readonly IBitacoraServices bitacoraServices;

        //Constructor con parámetros
        public FacturasController(DbContextBigFOOD pContext, GometaServices pGometaServices, IPdfServices pPdfServices, IEmailServices pEmailServices, IBitacoraServices pBitacoraServices)
        {
            this.dbContext = pContext;
            this.gometaServices = pGometaServices;
            this.pdfServices = pPdfServices;
            this.emailServices = pEmailServices;
            this.bitacoraServices = pBitacoraServices;
        }

        //Metodo encargado de obtener la lista de facturas
        [HttpGet]
        [Route("List")]
        public List<Factura> List()
        {
            return this.dbContext.Facturas.ToList();
        }

        //Método encargado de buscar una factura en específico
        [HttpGet]
        [Route("Search")]
        public IActionResult Search(int numero)
        {
            Factura temp = this.dbContext.Facturas.Find(numero);

            if (temp == null)
            {
                return NotFound($"No existe ninguna factura con el número {numero}");
            }

            return Ok(temp);
        }

        //Método encargado de crear una factura
        [HttpPost]
        [Route("CrearFactura")]
        public async Task<string> CrearFactura(FacturaDTO temp)
        {
            try
            {
                //Se valida si no se recibieron datos
                if (temp == null)
                {
                    return "No se recibieron datos de la factura";
                }

                //Se valida el tipo de pago
                if (temp.TipoPago.ToUpper() != "EFECTIVO" &&
                    temp.TipoPago.ToUpper() != "TARJETA" &&
                    temp.TipoPago.ToUpper() != "SINPE MOVIL")
                {
                    return "El tipo de pago debe ser EFECTIVO, TARJETA o SINPE MOVIL";
                }

                //Se valida la condición de la factura
                if (temp.Condicion.ToUpper() != "CONTADO" &&
                    temp.Condicion.ToUpper() != "CREDITO")
                {
                    return "La condición debe ser CONTADO o CREDITO";
                }

                //Se convierten los valores a mayúsculas
                temp.TipoPago = temp.TipoPago.ToUpper();
                temp.Condicion = temp.Condicion.ToUpper();

                //Se valida si la factura contiene productos
                if (temp.Detalle.Count == 0)
                {
                    return "La factura no contiene productos";
                }

                //Se busca el cliente en la base de datos
                Cliente cliente = this.dbContext.Clientes.FirstOrDefault(c => c.CedulaLegal.Equals(temp.CedulaCliente));

                //Se valida si el cliente existe
                if (cliente == null)
                {
                    return $"No existe ningún cliente con la cédula {temp.CedulaCliente}";
                }

                //Se valida si el cliente está activo
                if (!cliente.Estado)
                {
                    return $"El cliente {cliente.NombreCompleto} se encuentra inactivo";
                }

                //Variables para almacenar los totales de la factura
                decimal subtotalFactura = 0;
                decimal descuentoFactura = 0;
                decimal impuestoFactura = 0;
                decimal totalFactura = 0;

                //Se recorren los productos enviados en el detalle
                foreach (DetalleFacturaDTO item in temp.Detalle)
                {
                    //Se busca el producto
                    Producto producto = this.dbContext.Productos.Find(item.CodInterno);

                    //Se valida si existe
                    if (producto == null)
                    {
                        return $"No existe el producto {item.CodInterno}";
                    }

                    //Se valida existencia
                    if (producto.Existencia < item.Cantidad)
                    {
                        return $"No hay existencias suficientes para {producto.Descripcion}";
                    }

                    //Se calcula el subtotal del producto
                    decimal subtotalProducto = producto.PrecioVenta * item.Cantidad;

                    //Se calcula el descuento del producto
                    decimal descuentoProducto = subtotalProducto * (producto.Descuento / 100);

                    //Se calcula el impuesto del producto
                    decimal impuestoProducto = (subtotalProducto - descuentoProducto) * (producto.Impuesto / 100);

                    //Se acumulan los totales
                    subtotalFactura += subtotalProducto;
                    descuentoFactura += descuentoProducto;
                    impuestoFactura += impuestoProducto;
                }

                //Se calcula el total de la factura
                totalFactura = subtotalFactura - descuentoFactura + impuestoFactura;

                //Se consulta el tipo de cambio en Gometa
                decimal tipoCambio = await this.gometaServices.ConsultarTipoCambio();

                //Se calcula el total en dólares
                decimal totalDolares = totalFactura / tipoCambio;

                //Se determina el estado inicial de la factura
                string estadoFactura = "PAGADA";

                if (temp.Condicion.ToUpper() == "CREDITO")
                {
                    estadoFactura = "PENDIENTE";
                }

                //Se crea el encabezado de la factura
                Factura factura = new Factura
                {
                    Fecha = DateTime.Now,
                    CedulaCliente = temp.CedulaCliente,
                    Subtotal = subtotalFactura,
                    MontoDescuento = descuentoFactura,
                    MontoImpuesto = impuestoFactura,
                    Total = totalFactura,
                    Estado = estadoFactura,
                    UsuarioId = temp.UsuarioId,
                    TipoPago = temp.TipoPago,
                    Condicion = temp.Condicion
                };

                //Se almacena la factura
                this.dbContext.Facturas.Add(factura);
              
                //Se aplican los cambios
                this.dbContext.SaveChanges();

                //Se recorren nuevamente los productos para almacenar el detalle
                foreach (DetalleFacturaDTO item in temp.Detalle)
                {
                    //Se busca el producto
                    Producto producto = this.dbContext.Productos.Find(item.CodInterno);

                    //Se calcula el subtotal del producto
                    decimal subtotalProducto = producto.PrecioVenta * item.Cantidad;

                    //Se crea el detalle de factura
                    Det_Factura detalle = new Det_Factura
                    {
                        NumFactura = factura.Numero,
                        CodInterno = producto.CodigoInterno,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.PrecioVenta,
                        Subtotal = subtotalProducto,
                        PorImp = producto.Impuesto,
                        PorDescuento = producto.Descuento
                    };

                    //Se almacena el detalle
                    this.dbContext.Det_Facturas.Add(detalle);

                    //Se disminuye la existencia del producto
                    producto.Existencia -= item.Cantidad;

                    //Se actualiza el producto
                    this.dbContext.Productos.Update(producto);
                }

                //Se aplican los cambios del detalle y existencias
                this.dbContext.SaveChanges();

                //Si la factura es a crédito se crea una cuenta por cobrar
                if (temp.Condicion.ToUpper() == "CREDITO")
                {
                    CuentasPorCobrar cuenta = new CuentasPorCobrar
                    {
                        NumFactura = factura.Numero,
                        CedulaCliente = factura.CedulaCliente,
                        FechaFactura = factura.Fecha,
                        FechaRegistro = DateTime.Now,
                        MontoFactura = factura.Total,
                        UsuarioId = factura.UsuarioId,
                        Estado = "Pendiente"
                    };

                    this.dbContext.CuentasPorCobrar.Add(cuenta);

                    this.dbContext.SaveChanges();
                }

                //Lista para almacenar el detalle del PDF
                List<DetalleFacturaPdfDTO> detallePdf = new List<DetalleFacturaPdfDTO>();

                //Se recorren los productos para crear el detalle del PDF
                foreach (DetalleFacturaDTO item in temp.Detalle)
                {
                    //Se busca el producto
                    Producto producto = this.dbContext.Productos.Find(item.CodInterno);

                    //Se agrega el producto al detalle del PDF
                    detallePdf.Add(new DetalleFacturaPdfDTO
                    {
                        Descripcion = producto.Descripcion,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.PrecioVenta,
                        Subtotal = producto.PrecioVenta * item.Cantidad
                    });
                }

                //Se genera el PDF de la factura
                byte[] pdf = this.pdfServices.GenerarPdfFactura(
                    factura,
                    cliente,
                    detallePdf,
                    tipoCambio);

                //Asunto del correo
                string asunto = $"Factura #{factura.Numero} - BigFOOD";

                //Cuerpo del correo
                string cuerpo =
                    $@"<h2>Gracias por comprar en BigFOOD</h2>
                    <p>Estimado cliente {cliente.NombreCompleto}.</p>
                    <p>Adjunto encontrará la factura de su compra.</p>";

                //Se envía el correo al cliente
                await this.emailServices.EnviarCorreoConAdjuntoAsync(
                    cliente.Email,
                    asunto,
                    cuerpo,
                    pdf,
                    $"Factura_{factura.Numero}.pdf");

                //Se registra el evento en bitácora
                await this.bitacoraServices.RegistrarEvento(
                    "Facturas",
                    factura.UsuarioId,
                    "INSERT",
                    $"Factura #{factura.Numero}");

                //Se retorna el resultado
                return $"Factura #{factura.Numero} creada correctamente y enviada al correo {cliente.Email}";
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }

        //Metodo encargdo de anular una factura 
        [HttpDelete]
        [Route("AnularFactura")]
        public async Task<string> AnularFactura(int numero)
        {
            try
            {
                Factura factura = this.dbContext.Facturas.Find(numero);

                if (factura == null)
                {
                    return $"No existe la factura #{numero}";
                }

                factura.Estado = "ANULADA";

                this.dbContext.Facturas.Update(factura);

                this.dbContext.SaveChanges();

                //Se registra el evento en bitácora
                await this.bitacoraServices.RegistrarEvento(
                    "Facturas",
                    factura.UsuarioId,
                    "ANULAR",
                    $"Factura #{factura.Numero}");

                return $"Factura #{numero} anulada correctamente";
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }

        //Método encargado de marcar una cuenta por cobrar como pagada
        [HttpPut]
        [Route("PagarCuenta")]
        public async Task<string> PagarCuenta(int numeroFactura)
        {
            try
            {
                CuentasPorCobrar cuenta = this.dbContext.CuentasPorCobrar.Find(numeroFactura);

                if (cuenta == null)
                {
                    return $"No existe ninguna cuenta por cobrar para la factura #{numeroFactura}";
                }

                if (cuenta.Estado.ToUpper() == "PAGADO")
                {
                    return $"La factura #{numeroFactura} ya se encuentra pagada";
                }

                cuenta.Estado = "Pagado";

                //Se actualiza el estado de la factura
                Factura factura = this.dbContext.Facturas.Find(numeroFactura);

                if (factura != null)
                {
                    factura.Estado = "PAGADA";

                    this.dbContext.Facturas.Update(factura);
                }

                this.dbContext.CuentasPorCobrar.Update(cuenta);

                this.dbContext.SaveChanges();

                return $"La factura #{numeroFactura} fue marcada como pagada";
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }
    }
}