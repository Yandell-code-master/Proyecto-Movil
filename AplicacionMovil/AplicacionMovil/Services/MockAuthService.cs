using System.Text;
using System.Text.Json;

namespace AplicacionMovil.Services;


public class MockAuthService
{
    private readonly HttpClient _httpClient;

    // NOTA PERSONAL: Si testeo con emulador local de Android, usar "http://10.0.2.2:PUERTO/login"
    // Si ya está subido al servidor de Somee u otro hosting, cambiar por la URL pública.
    private const string LoginUrl = "http://10.0.2.2:5119/login";

    public MockAuthService()
    {
        // Inicializo el cliente HTTP 
        _httpClient = new HttpClient();
    }

    /// <summary>
    /// Envía las credenciales del usuario a la API y maneja la respuesta del token.
    /// </summary>
    public async Task<AuthorizationResponse> LoginAsync(string email, string password)
    {
        // 1. Armo el objeto LoginDTO con el formato exacto de la API (Email, Password)
        var loginDto = new
        {
            Email = email,
            Password = password
        };

        // 2. Serializo el objeto a JSON y lo preparo con formato UTF-8 y application/json
        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
            {
            // 3. Hago el fetch (POST) directo al /login del endpoint la API
            var response = await _httpClient.PostAsync(LoginUrl, content);

            // Leo el string que me devuelve el servidor 
            var responseString = await response.Content.ReadAsStringAsync();

            // Configuro para que ignore mayúsculas/minúsculas al mapear el JSON 
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // 4. Si el servidor responde bien
            if (response.IsSuccessStatusCode)
            {
                // Mapeo la respuesta JSON a mi clase AuthorizationResponse
                var authResult = JsonSerializer.Deserialize<AuthorizationResponse>(responseString, options);

                // Si la API me confirmó que Result es true y viene el Token JWT válido
                if (authResult != null && authResult.Result && !string.IsNullOrEmpty(authResult.Token))
                {
                    // Almaceno el token en las preferencias nativas del dispositivo móvil para usarlo luego
                    Preferences.Default.Set("jwt_token", authResult.Token);
                }

                return authResult;
            }
            else
            {

                // Intento deserializar el JSON de error del controlador para extraer el string de 'Msj'
                var errorResult = JsonSerializer.Deserialize<AuthorizationResponse>(responseString, options);

                // Si por alguna razón el JSON viene vacío, tiro un mensaje por defecto
                return errorResult ?? new AuthorizationResponse { Result = false, Msj = "Error al intentar autenticar." };
            }
        }
        catch (Exception ex)
        {
            // 6. Captura de errores críticos de infraestructura (Servidor apagado, caída de internet, etc)
            System.Diagnostics.Debug.WriteLine($"[AuthService Error] Fallo de red crítico: {ex.Message}");

            return new AuthorizationResponse
            {
                Result = false,
                Msj = "No se pudo establecer conexión con el servidor de Big Food Services. Verifique su red."
            };
        }
    }
}

/// <summary>
/// Réplica exacta de la estructura 'AuthorizationResponse' que definió mi API de seguridad.
/// Nos sirve para mapear limpiamente las propiedades Token, Msj y Result del JSON.
/// </summary>
public class AuthorizationResponse
{
    public string Token { get; set; }
    public string Msj { get; set; }
    public bool Result { get; set; }
}