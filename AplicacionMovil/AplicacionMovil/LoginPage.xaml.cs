using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using AplicacionMovil.Services;

namespace AplicacionMovil;


public partial class LoginPage : ContentPage
{
    private readonly MockAuthService _authService;

    public LoginPage()
    {
        InitializeComponent();

        // Inicializo mi servicio HTTP real
        _authService = new MockAuthService();

        //  Recupero el usuario y contraseña guardados si se marcó el "Remember me" en la sesión anterior
        var savedUsername = Preferences.Get("RememberedUsername", string.Empty);
        var savedPassword = Preferences.Get("RememberedPassword", string.Empty);

        if (!string.IsNullOrEmpty(savedUsername))
        {
            txtUsername.Text = savedUsername;
            txtPassword.Text = savedPassword;
            chkRemember.IsChecked = true;
        }
    }



    private void OnUsernameFocused(object? sender, FocusEventArgs e)
    {
        underlineUsername.Color = (Color)Application.Current!.Resources["Primary"];
    }

    private void OnUsernameUnfocused(object? sender, FocusEventArgs e)
    {
        underlineUsername.Color = (Color)Application.Current!.Resources["OutlineVariant"];
    }

    private void OnPasswordFocused(object? sender, FocusEventArgs e)
    {
        underlinePassword.Color = (Color)Application.Current!.Resources["Primary"];
    }

    private void OnPasswordUnfocused(object? sender, FocusEventArgs e)
    {
        underlinePassword.Color = (Color)Application.Current!.Resources["OutlineVariant"];
    }

    /// <summary>
    /// Cambia la visibilidad del texto de la contraseña y alterna el icono del ojo.
    /// </summary>
    private void OnTogglePasswordVisibility(object? sender, TappedEventArgs e)
    {
        txtPassword.IsPassword = !txtPassword.IsPassword;
        
        lblTogglePassword.Text = txtPassword.IsPassword ? "\U0001F441" : "\U0001F441\u200D\U0001F5E8";
    }
 
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        
        var email = txtUsername.Text?.Trim();
        var password = txtPassword.Text?.Trim();

    
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error de Validación", "Por favor ingrese tanto el correo como la contraseña.", "OK");
            return;
        }

        // Bloqueo el botón para evitar múltiples llamadas en redes lentas
        btnLogin.IsEnabled = false;
        btnLogin.Text = "LOGGING IN...";

        try
        {
            //  Invoco mi AuthService pasándole las credenciales del formulario
            AuthorizationResponse respuesta = await _authService.LoginAsync(email, password);

            // Evalúo si la API me dio  el Token 
            if (respuesta != null && respuesta.Result)
            {
                // Manejo del "Remember me" en el dispositivo
                if (chkRemember.IsChecked)
                {
                    Preferences.Set("RememberedUsername", email);
                    Preferences.Set("RememberedPassword", password);
                }
                else
                {
                    // Si el usuario lo desmarcó, limpio el almacenamiento local
                    Preferences.Remove("RememberedUsername");
                    Preferences.Remove("RememberedPassword");
                }

                // Limpio los campos de texto
                txtUsername.Text = string.Empty;
                txtPassword.Text = string.Empty;

                // Mensaje de éxito informativo antes de saltar
                await DisplayAlert("Bienvenido", $"{respuesta.Msj}", "OK");

                // 3. REDIRECCIÓN: Saltamos a las facturas usando las rutas declaradas en el AppShell
                await Shell.Current.GoToAsync($"//{nameof(BigFood_Facturas)}");
            }
            else
            {
                // Muestro el error 
                string mensajeError = respuesta?.Msj ?? "Credenciales inválidas o usuario inactivo.";
                await DisplayAlert("Login Fallido", mensajeError, "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Login Click Error] Error crítico: {ex.Message}");
            await DisplayAlert("Error", "Ocurrió un problema de comunicación con el servidor.", "OK");
        }
        finally
        {
            // Desbloqueo el botón siempre al finalizar la tarea
            btnLogin.IsEnabled = true;
            btnLogin.Text = "LOG IN";
        }
    }

    /// <summary>
    /// Manejo del link para restablecer el acceso.
    /// </summary>
    private async void OnForgotAccessTapped(object? sender, TappedEventArgs e)
    {
        await DisplayAlert("Restablecer Acceso", "Por favor contacte al administrador para restablecer sus credenciales de acceso.", "OK");
    }
}