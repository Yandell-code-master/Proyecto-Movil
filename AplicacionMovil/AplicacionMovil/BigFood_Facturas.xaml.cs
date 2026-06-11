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
    // Colección reactiva que el CollectionView del XAML va a leer 
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

        // Llamamos a la API para traer los datos 
        await CargarFacturasAsync();
    }

    private async Task CargarFacturasAsync()
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
            
                string url = "http://10.0.2.2:5173/Facturas/List";

                // Consumimos el endpoint [HttpGet("List")] del controlador
                var lista = await client.GetFromJsonAsync<List<FacturaModel>>(url);

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
                // Alerta por si la API está apagada
                await DisplayAlert("Error de Conexión", $"No se pudieron obtener las facturas: {ex.Message}", "OK");
            }
        }
    }
}