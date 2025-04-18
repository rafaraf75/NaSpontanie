using System.Net.Http.Headers;
using System.Text.Json;
using NaSpontanie.MAUI.Dtos;

namespace NaSpontanie.MAUI;

public partial class EventListPage : ContentPage
{
    // Komenda wywoływana po kliknięciu w wydarzenie na liście
    public Command<EventDto> OnItemTappedCommand { get; }

    public EventListPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Obsługa kliknięcia w element listy
        OnItemTappedCommand = new Command<EventDto>(async (selectedEvent) =>
        {
            if (selectedEvent == null)
            {
                Console.WriteLine("Wybrany event jest null.");
                return;
            }

            Console.WriteLine("Kliknięto w wydarzenie: " + selectedEvent.Title);

            // Przejście do szczegółów wydarzenia, przekazując wybrany obiekt
            await Shell.Current.GoToAsync(nameof(EventDetailPage), true, new Dictionary<string, object>
            {
                { "SelectedEvent", selectedEvent }
            });
        });

        _ = LoadEvents(); // Pierwsze ładowanie danych przy inicjalizacji
    }

    // Odśwież dane po ponownym wejściu na stronę
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadEvents();
    }

    // Pobiera wydarzenia z API i przypisuje do listy
    private async Task LoadEvents()
    {
        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.18.10:5206")
            };

            // Dołączenie tokena autoryzacyjnego jeśli jest
            var token = Preferences.Get("auth_token", null);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Błąd", "Nie udało się załadować wydarzeń.", "OK");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<EventDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (events == null)
            {
                await DisplayAlert("Błąd", "Brak wydarzeń do wyświetlenia.", "OK");
                return;
            }

            // Oblicz dystans do każdego wydarzenia od lokalizacji użytkownika
            var userLat = Preferences.Get("latitude", 0.0);
            var userLon = Preferences.Get("longitude", 0.0);

            foreach (var ev in events)
            {
                ev.DistanceKm = Math.Round(CalculateDistance(userLat, userLon, ev.Latitude, ev.Longitude), 2);
            }

            // Ustaw listę wydarzeń posortowaną według dystansu
            EventsCollection.ItemsSource = events
                .OrderBy(e => e.DistanceKm)
                .ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", ex.Message, "OK");
        }
    }

    // Liczy dystans między dwoma punktami (Haversine formula)
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // promień Ziemi w kilometrach
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
