using API_BigFOOD.DTOs;
using API_BigFOOD.Models;
using API_BigFOOD.Services;
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
                    //Si el cliente existe pero está inactivo se reactiva
                    if (!aux.Estado)
                    {
                        //Se valida si ya existe otro cliente con el mismo correo
                        Cliente clienteCorreo = this.dbContext.Clientes.FirstOrDefault(c => c.Email.Equals(temp.Email) && c.CedulaLegal != temp.CedulaLegal);

                        if (clienteCorreo != null)
                        {
                            return $"Ya existe otro cliente con el correo {temp.Email}";
                        }

                        aux.Email = temp.Email;
                        aux.UsuarioId = temp.UsuarioId;
                        aux.Estado = true;

                        this.dbContext.SaveChanges();

                        //Se registra el evento en bitácora
                        await this.bitacoraServices.RegistrarEvento(
                            "Clientes",
                            temp.UsuarioId,
                            "UPDATE",
                            $"Cliente reactivado: {aux.NombreCompleto}");

                        return $"Cliente {aux.NombreCompleto} reactivado correctamente...";
                    }

                    return $"Ya existe un cliente con la cédula {temp.CedulaLegal}";
                }

                //Se valida si ya existe un cliente con el mismo correo
                Cliente clienteCorreoActivo = this.dbContext.Clientes.FirstOrDefault(c => c.Email.Equals(temp.Email));

                if (clienteCorreoActivo != null)
                {
                    return $"Ya existe un cliente con el correo {temp.Email}";
                }

                //Se consulta la información del cliente en Gometa
                ClienteGometaDTO clienteGometa =
                    await this.gometaServices.ConsultarCedula(temp.CedulaLegal);

                //Si existe información en Gometa se completa automáticamente
                if (clienteGometa != null)
                {
                    //Se asigna el nombre completo obtenido desde Gometa
                    temp.NombreCompleto = clienteGometa.Nombre;

                    //Se asigna el tipo de cédula obtenido desde Gometa
                    if (clienteGometa.TipoIdentificacion == "FISICA")
                    {
                        temp.TipoCedula = "FISICA";
                    }
                    else if (clienteGometa.TipoIdentificacion == "JURIDICA")
                    {
                        temp.TipoCedula = "JURIDICA";
                    }
                    else if (clienteGometa.TipoIdentificacion == "DIMEX/NITE")
                    {
                        temp.TipoCedula = "DIMEX";
                    }
                    else
                    {
                        return "Tipo de identificación no soportado";
                    }
                }
                else
                {
                    //Si no existe en Gometa se validan los datos ingresados manualmente

                    if (string.IsNullOrWhiteSpace(temp.NombreCompleto))
                    {
                        return "Debe indicar el nombre completo del cliente";
                    }

                    if (string.IsNullOrWhiteSpace(temp.TipoCedula))
                    {
                        return "Debe indicar el tipo de cédula";
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
                }

                //Se asignan los valores por defecto
                temp.FechaRegistro = DateTime.Now;
                temp.Estado = true;

                //Se almacena el cliente
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
        public async Task<string> Update(Cliente temp)
        {
            try
            {
                if (temp != null)
                {
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

                    aux.Email = temp.Email;
                    aux.Estado = temp.Estado;

                    this.dbContext.Clientes.Update(aux);

                    this.dbContext.SaveChanges();

                    //Se registra el evento en bitácora
                    await this.bitacoraServices.RegistrarEvento(
                        "Clientes",
                        temp.UsuarioId,
                        "UPDATE",
                        $"Cliente: {aux.NombreCompleto}");

                    return $"Cambios aplicados correctamente a {aux.NombreCompleto}";
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

                    //Se valida si el cliente ya se encuentra inactivo
                    if (!temp.Estado)
                    {
                        return $"El cliente {temp.NombreCompleto} ya se encuentra inactivo";
                    }

                    //Se realiza la eliminación lógica del cliente
                    temp.Estado = false;

                    //Se guardan los cambios
                    this.dbContext.SaveChanges();

                    //Se registra el evento en bitácora
                    await this.bitacoraServices.RegistrarEvento(
                        "Clientes",
                        temp.UsuarioId,
                        "DELETE",
                        $"Cliente: {temp.NombreCompleto}");

                    return $"Cliente {temp.NombreCompleto} desactivado correctamente...";
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
    }
}
