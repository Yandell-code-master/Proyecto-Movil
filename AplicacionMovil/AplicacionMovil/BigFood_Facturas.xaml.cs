using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace AplicacionMovil;

public partial class BigFood_Facturas : ContentPage
{
    // Colección reactiva que el CollectionView del XAML va a leer automáticamente
    public ObservableCollection<FacturaModel> Facturas { get; set; } = new ObservableCollection<FacturaModel>();

    public BigFood_Facturas()
    {
        InitializeComponent();

        // Conectamos nuestra lista de C# con el CollectionView del XAML
        ListaFacturas.ItemsSource = Facturas;
    }

    // Este método de .NET MAUI se ejecuta solo cada vez que la pantalla aparece en el celular
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Llamamos a la API para traer los datos frescos
        await CargarFacturasAsync();
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Cerrar Sesión", "¿Está seguro de que desea salir?", "Sí", "Cancelar");
        if (!confirm) return;

        Preferences.Remove("jwt_token");
        Preferences.Remove("RememberedUsername");
        Preferences.Remove("RememberedPassword");
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }

    private async Task CargarFacturasAsync()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string url = "http://BigFOOD-API.somee.com/Facturas/List";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var token = Preferences.Get("jwt_token", string.Empty);
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var lista = await response.Content.ReadFromJsonAsync<List<FacturaModel>>();

                if (lista != null)
                {
                    // Limpiamos la lista vieja para no duplicar datos en la pantalla
                    Facturas.Clear();

                    // Inyectamos las facturas nuevas que llegaron listas de la API
                    foreach (var factura in lista)
                    {
                        Facturas.Add(factura);
                    }
                }
            }
            catch (Exception ex)
            {
                // Alerta por si la API está apagada, el puerto está mal o el emulador no tiene red
                await DisplayAlert("Error de Conexión", $"No se pudieron obtener las facturas: {ex.Message}", "OK");
            }
        }
    }
}