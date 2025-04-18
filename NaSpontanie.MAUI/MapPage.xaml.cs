using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Net.Http.Headers;
using System.Text.Json;
using NaSpontanie.MAUI.Dtos;

namespace NaSpontanie.MAUI;

public partial class MapPage : ContentPage
{
    private Pin? _lastClickedPin = null;

    public MapPage()
    {
        InitializeComponent();
        LoadMap(); // Inicjalizacja mapy przy starcie strony
    }

    // �aduje map� oraz wydarzenia w okolicy u�ytkownika
    private async void LoadMap()
    {
        try
        {
            // Pobranie wsp�rz�dnych u�ytkownika z zapisanych preferencji
            var latitude = Preferences.Get("latitude", 0.0);
            var longitude = Preferences.Get("longitude", 0.0);

            if (latitude == 0.0 && longitude == 0.0)
            {
                await DisplayAlert("B��d", "Brak zapisanej lokalizacji u�ytkownika.", "OK");
                return;
            }

            // Ustawienie widoku mapy na lokalizacj� u�ytkownika
            var userLocation = new Location(latitude, longitude);
            var mapSpan = MapSpan.FromCenterAndRadius(userLocation, Distance.FromKilometers(2));
            UserMap.MoveToRegion(mapSpan);
            UserMap.Pins.Clear();

            // Pinezka z lokalizacj� u�ytkownika
            var userPin = new Pin
            {
                Label = "Tutaj jeste�",
                Address = "Twoja lokalizacja",
                Location = userLocation
            };
            UserMap.Pins.Add(userPin);

            // Konfiguracja klienta HTTP
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.18.10:5206")
            };

            var token = Preferences.Get("auth_token", null);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            // Pobranie listy wydarze� z API
            var response = await httpClient.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("B��d", "Nie uda�o si� za�adowa� wydarze�.", "OK");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<EventDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (events == null)
            {
                await DisplayAlert("B��d", "Brak wydarze� do wy�wietlenia.", "OK");
                return;
            }

            // Dodanie pinezek do mapy dla ka�dego wydarzenia
            foreach (var ev in events)
            {
                var eventPin = new Pin
                {
                    Label = $"{ev.Title} ({ev.InterestName})",
                    Address = string.IsNullOrWhiteSpace(ev.Description)
                        ? "Brak opisu"
                        : (ev.Description.Length > 60 ? ev.Description.Substring(0, 60) + "..." : ev.Description),
                    Location = new Location(ev.Latitude, ev.Longitude),
                    Type = PinType.Place
                };

                // Pokazanie info okna po klikni�ciu pinezki
                eventPin.MarkerClicked += (s, e) =>
                {
                    e.HideInfoWindow = false;
                };

                // Po klikni�ciu info okna � przej�cie do szczeg��w wydarzenia
                eventPin.InfoWindowClicked += async (s, e) =>
                {
                    await Shell.Current.GoToAsync(nameof(EventDetailPage), true, new Dictionary<string, object>
                    {
                        { "SelectedEvent", ev }
                    });
                };

                UserMap.Pins.Add(eventPin);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B��d", $"Nie uda�o si� za�adowa� mapy: {ex.Message}", "OK");
        }
    }
}
