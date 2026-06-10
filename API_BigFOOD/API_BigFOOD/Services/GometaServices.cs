using API_BigFOOD.DTOs;
using System.Text.Json;

namespace API_BigFOOD.Services
{
    public class GometaServices
    {
        //Variable para manejar la comunicación con el API de Gometa
        private readonly HttpClient httpClient;

        public GometaServices()
        {
            //Se instancia el objeto HttpClient
            httpClient = new HttpClient();

            //Se configura la URL base del API de Gometa
            httpClient.BaseAddress = new Uri("https://apis.gometa.org/");
        }

        //Método encargado de consultar una persona por cédula en Gometa
        public async Task<ClienteGometaDTO> ConsultarCedula(string cedula)
        {
            //Se realiza la consulta al API de Gometa
            var response = await httpClient.GetAsync($"cedulas/{cedula}");

            //Se valida si la respuesta fue incorrecta
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            //Se obtiene el contenido JSON retornado por el API
            string json = await response.Content.ReadAsStringAsync();

            //Se convierte el texto JSON en un documento manipulable
            JsonDocument documento = JsonDocument.Parse(json);

            //Se obtiene el elemento raíz del JSON
            JsonElement root = documento.RootElement;

            //Se obtiene la cantidad de resultados encontrados
            int cantidadResultados = root.GetProperty("resultcount").GetInt32();

            //Se valida si no existen resultados
            if (cantidadResultados == 0)
            {
                return null;
            }

            //Se obtiene el primer resultado encontrado
            JsonElement resultado = root.GetProperty("results")[0];

            //Se crea el objeto DTO con los datos requeridos
            ClienteGometaDTO cliente = new ClienteGometaDTO
            {
                Cedula = root.GetProperty("cedula").GetString(),
                Nombre = resultado.GetProperty("fullname").GetString(),
                TipoIdentificacion = resultado.GetProperty("guess_type").GetString()
            };

            //Se retorna la información del cliente
            return cliente;
        }

        //Método encargado de consultar el tipo de cambio
        public async Task<decimal> ConsultarTipoCambio()
        {
            var response = await httpClient.GetAsync("tdc/tdc.json");

            if (!response.IsSuccessStatusCode)
            {
                return 0;
            }

            string json = await response.Content.ReadAsStringAsync();

            JsonDocument documento = JsonDocument.Parse(json);

            JsonElement root = documento.RootElement;

            return root.GetProperty("venta").GetDecimal();
        }
    }
}