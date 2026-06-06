using API_BigFOOD.Models;
using API_BigFOOD.Services;
using Microsoft.AspNetCore.Authorization; // Librería para uso de EndPoint protegidos
using Microsoft.AspNetCore.Mvc;

namespace API_BigFOOD.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductosController : ControllerBase
    {
        //Variable para manejar la referencia del ORM
        private readonly DbContextBigFOOD dbContext = null;

        private readonly IBitacoraServices bitacoraServices;

        //Constructor con parámetros
        public ProductosController(DbContextBigFOOD pContext, IBitacoraServices pBitacoraServices)
        {
            this.dbContext = pContext;
            this.bitacoraServices = pBitacoraServices;
        }

        //Método para obtener todos los productos
        [HttpGet]
        [Route("List")]
        public List<Producto> List()
        {
            return this.dbContext.Productos.ToList();
        }

        //Método encargado de consultar un producto
        [HttpGet]
        [Route("Search")]
        public IActionResult Search(int codigoInterno)
        {
            Producto temp = this.dbContext.Productos.Find(codigoInterno);

            if (temp == null)
            {
                return NotFound($"No existe ningún producto con el código interno {codigoInterno}");
            }

            return Ok(temp);
        }

        //Método para almacenar los datos de un producto
        [HttpPost]
        [Route("Save")]
        [Authorize]
        public async Task<string> Save(Producto temp)
        {
            try
            {
                if (temp == null)
                {
                    return "No se ha indicado ningún dato";
                }

                //Se valida si ya existe un producto con la misma descripción
                Producto aux = this.dbContext.Productos.FirstOrDefault(p => p.Descripcion.Equals(temp.Descripcion));

                if (aux != null)
                {
                    return $"Ya existe un producto con la descripción {temp.Descripcion}";
                }
                else
                {
                    this.dbContext.Productos.Add(temp);

                    this.dbContext.SaveChanges();

                    //Se registra el evento en bitácora
                    await this.bitacoraServices.RegistrarEvento(
                        "Productos",
                        temp.UsuarioId,
                        "INSERT",
                        $"Producto: {temp.Descripcion}");

                    return $"Producto {temp.Descripcion} almacenado correctamente...";
                }
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }

        //Método encargado de modificar los datos de un producto
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public async Task<string> Update(Producto temp)
        {
            try
            {
                if (temp != null)
                {
                    Producto aux = this.dbContext.Productos.Find(temp.CodigoInterno);

                    if (aux == null)
                    {
                        return $"No existe ningún producto con el código interno {temp.CodigoInterno}";
                    }

                    //Se valida si ya existe otro producto con la misma descripción
                    Producto productoDescripcion = this.dbContext.Productos.FirstOrDefault(p => p.Descripcion.Equals(temp.Descripcion) && p.CodigoInterno != temp.CodigoInterno);

                    if (productoDescripcion != null)
                    {
                        return $"Ya existe otro producto con la descripción {temp.Descripcion}";
                    }

                    // Mapeo de los campos modificables del producto
                    aux.CodigoBarra = temp.CodigoBarra;
                    aux.Descripcion = temp.Descripcion;
                    aux.PrecioVenta = temp.PrecioVenta;
                    aux.Descuento = temp.Descuento;
                    aux.Impuesto = temp.Impuesto;
                    aux.UnidadMedida = temp.UnidadMedida;
                    aux.PrecioCompra = temp.PrecioCompra;
                    aux.UsuarioId = temp.UsuarioId;
                    aux.Existencia = temp.Existencia;

                    this.dbContext.Productos.Update(aux);

                    this.dbContext.SaveChanges();

                    //Se registra el evento en bitácora
                    await this.bitacoraServices.RegistrarEvento(
                        "Productos",
                        temp.UsuarioId,
                        "UPDATE",
                        $"Producto: {temp.Descripcion}");

                    return $"Cambios aplicados correctamente a {temp.Descripcion}";
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
        public async Task<string> Delete(int codigoInterno)
        {
            try
            {
                Producto temp = this.dbContext.Productos.Find(codigoInterno);

                if (temp != null)
                {
                    this.dbContext.Productos.Remove(temp);

                    this.dbContext.SaveChanges();

                    //Se registra el evento en bitácora
                    await this.bitacoraServices.RegistrarEvento(
                        "Productos",
                        temp.UsuarioId,
                        "DELETE",
                        $"Producto: {temp.Descripcion}");

                    return $"Producto {temp.Descripcion} eliminado correctamente...";
                }
                else
                {
                    return $"No existe un producto con el código interno {codigoInterno}";
                }
            }
            catch (Exception ex)
            {
                return "Error " + ex.Message.ToString();
            }
        }
    }
}
