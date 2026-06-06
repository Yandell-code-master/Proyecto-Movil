using API_BigFOOD.DTOs;
using API_BigFOOD.Models;
using API_BigFOOD.Services;
using Microsoft.AspNetCore.Authorization; // Librería para uso de EndPoint protegidos
using Microsoft.AspNetCore.Mvc;

namespace API_BigFOOD.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientesController : ControllerBase
    {
        //Variable para manejar la referencia del ORM
        private readonly DbContextBigFOOD dbContext = null;
        //Variable para menejar la referencia de los servicios de Gometa
        private readonly GometaServices gometaServices;
        //Variable para menejar los eventos de la bitacora
        private readonly IBitacoraServices bitacoraServices;

        //Constructor con parámetros
        public ClientesController(DbContextBigFOOD pContext,GometaServices pGometaServices, IBitacoraServices pBitacoraServices)
        {
            this.dbContext = pContext;
            this.gometaServices = pGometaServices;
            this.bitacoraServices = pBitacoraServices;
        }

        //Método para obtener todos los clientes
        [HttpGet]
        [Route("List")]
        public List<Cliente> List()
        {
            return this.dbContext.Clientes.ToList();
        }

        //Método encargado de consultar un cliente
        [HttpGet]
        [Route("Search")]
        public IActionResult Search(string cedula)
        {
            Cliente temp = this.dbContext.Clientes.Find(cedula);

            if (temp == null)
            {
                return NotFound($"No existe ningún cliente con la cédula {cedula}");
            }

            return Ok(temp);
        }

        //Método para almacenar los datos de un cliente
        [HttpPost]
        [Route("Save")]
        [Authorize]
        public async Task<string> Save(Cliente temp)
        {
            try
            {
                if (temp == null)
                {
                    return "No se ha indicado ningún dato";
                }

                //Se valida si ya existe un cliente con esa cédula
                Cliente aux = this.dbContext.Clientes.Find(temp.CedulaLegal);

                if (aux != null)
                {
                    return $"Ya existe un cliente con la cédula {temp.CedulaLegal}";
                }

                //Se valida si ya existe un cliente con el mismo correo
                Cliente clienteCorreo = this.dbContext.Clientes.FirstOrDefault(c => c.Email.Equals(temp.Email));

                if (clienteCorreo != null)
                {
                    return $"Ya existe un cliente con el correo {temp.Email}";
                }

                //Se valida el tipo de cédula
                if (temp.TipoCedula.ToUpper() != "FISICA" &&
                    temp.TipoCedula.ToUpper() != "JURIDICA" &&
                    temp.TipoCedula.ToUpper() != "DIMEX")
                {
                    return "El tipo de cédula debe ser FISICA, JURIDICA o DIMEX";
                }

                //Se convierte el tipo de cédula a mayúsculas
                temp.TipoCedula = temp.TipoCedula.ToUpper();

                temp.FechaRegistro = DateTime.Now;
                temp.Estado = true;

                this.dbContext.Clientes.Add(temp);

                this.dbContext.SaveChanges();

                //Se registra el evento en bitácora
                await this.bitacoraServices.RegistrarEvento(
                    "Clientes",
                    temp.UsuarioId,
                    "INSERT",
                    $"Cliente: {temp.NombreCompleto}");

                return $"Cliente {temp.NombreCompleto} almacenado correctamente...";
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }

        //Método encargado de modificar los datos de un cliente
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public async Task<string> Update(Cliente temp)
        {
            try
            {
                if (temp != null)
                {
                    //Se valida el tipo de cédula
                    if (temp.TipoCedula.ToUpper() != "FISICA" &&
                        temp.TipoCedula.ToUpper() != "JURIDICA" &&
                        temp.TipoCedula.ToUpper() != "DIMEX")
                    {
                        return "El tipo de cédula debe ser FISICA, JURIDICA o DIMEX";
                    }

                    //Se convierte el tipo de cédula a mayúsculas
                    temp.TipoCedula = temp.TipoCedula.ToUpper();

                    Cliente aux = this.dbContext.Clientes.Find(temp.CedulaLegal);

                    if (aux == null)
                    {
                        return $"No existe ningún cliente con la cédula {temp.CedulaLegal}";
                    }

                    //Se valida si ya existe otro cliente con el mismo correo
                    Cliente clienteCorreo = this.dbContext.Clientes.FirstOrDefault(c => c.Email.Equals(temp.Email) && c.CedulaLegal != temp.CedulaLegal);

                    if (clienteCorreo != null)
                    {
                        return $"Ya existe otro cliente con el correo {temp.Email}";
                    }

                    aux.TipoCedula = temp.TipoCedula;
                    aux.NombreCompleto = temp.NombreCompleto;
                    aux.Email = temp.Email;
                    aux.Estado = temp.Estado;
                    aux.UsuarioId = temp.UsuarioId;

                    this.dbContext.Clientes.Update(aux);

                    this.dbContext.SaveChanges();

                    //Se registra el evento en bitácora
                    await this.bitacoraServices.RegistrarEvento(
                        "Clientes",
                        temp.UsuarioId,
                        "UPDATE",
                        $"Cliente: {temp.NombreCompleto}");

                    return $"Cambios aplicados correctamente a {temp.NombreCompleto}";
                }
                else
                {
                    return "No se permiten datos en blanco";
                }
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }

        //Método encargado del proceso eliminar
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public async Task<string> Delete(string cedula)
        {
            try
            {
                Cliente temp = this.dbContext.Clientes.Find(cedula);

                if (temp != null)
                {
                    //Se valida si el cliente tiene cuentas pendientes
                    bool tienePendientes = this.dbContext.CuentasPorCobrar.Any(c => c.CedulaCliente == cedula && c.Estado == "PENDIENTE");

                    if (tienePendientes)
                    {
                        return $"No se puede eliminar el cliente {temp.NombreCompleto} porque tiene facturas pendientes de pago";
                    }

                    //Se obtienen las facturas del cliente
                    List<Factura> facturas = this.dbContext.Facturas.Where(f => f.CedulaCliente == cedula).ToList();

                    //Se eliminan los detalles de las facturas
                    foreach (Factura factura in facturas)
                    {
                        List<Det_Factura> detalles = this.dbContext.Det_Facturas.Where(d => d.NumFactura == factura.Numero).ToList();

                        this.dbContext.Det_Facturas.RemoveRange(detalles);
                    }

                    //Se guardan los cambios
                    this.dbContext.SaveChanges();

                    //Se eliminan las cuentas por cobrar
                    List<CuentasPorCobrar> cuentas = this.dbContext.CuentasPorCobrar.Where(c => c.CedulaCliente == cedula).ToList();

                    this.dbContext.CuentasPorCobrar.RemoveRange(cuentas);

                    //Se guardan los cambios
                    this.dbContext.SaveChanges();

                    //Se eliminan las facturas
                    this.dbContext.Facturas.RemoveRange(facturas);

                    //Se guardan los cambios
                    this.dbContext.SaveChanges();

                    //Se elimina el cliente
                    this.dbContext.Clientes.Remove(temp);

                    //Se guardan los cambios
                    this.dbContext.SaveChanges();

                    //Se registra el evento en bitácora
                    await this.bitacoraServices.RegistrarEvento(
                        "Clientes",
                        temp.UsuarioId,
                        "DELETE",
                        $"Cliente: {temp.NombreCompleto}");

                    return $"Cliente {temp.NombreCompleto} eliminado correctamente...";
                }
                else
                {
                    return $"No existe un cliente con la cédula {cedula}";
                }
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }

        //Metodo encargado de consumir el API de Gometa y buscar un cliente por cédula
        [HttpGet]
        [Route("ConsultarCedulaGometa")]
        public async Task<IActionResult> ConsultarCedulaGometa(string cedula)
        {
            ClienteGometaDTO cliente = await this.gometaServices.ConsultarCedula(cedula);

            if (cliente == null)
            {
                return NotFound("No existe ninguna persona con esa cédula");
            }

            return Ok(cliente);
        }
    }
}
