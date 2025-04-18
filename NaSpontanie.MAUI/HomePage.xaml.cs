using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using NaSpontanie.MAUI.Models;
using NaSpontanie.MAUI.Services;

namespace NaSpontanie.MAUI;

public partial class HomePage : ContentPage
{
    // Tworzy nowego klienta HTTP z konfiguracją z ApiService
    private HttpClient _httpClient => ApiService.CreateClient();

    public HomePage()
    {
        InitializeComponent();
    }

    // Gdy strona się pojawia, załaduj dane użytkownika
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUser();
    }

    // Pobiera dane użytkownika i ustawia powitanie
    private async void LoadUser()
    {
        try
        {
            var token = Preferences.Get("auth_token", null);
            var userId = Preferences.Get("user_id", null);

            Debug.WriteLine("=== START LoadUser ===");
            Debug.WriteLine($"Token: {token}");
            Debug.WriteLine($"UserId: {userId}");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                WelcomeLabel.Text = "Witaj";
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync($"api/users/{userId}");
            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"[LoadUser] Odpowiedź z API: {json}");

            if (response.IsSuccessStatusCode)
            {
                var user = JsonSerializer.Deserialize<UserModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                WelcomeLabel.Text = user?.Username != null
                    ? $"Witaj, {user.Username}"
                    : "Witaj";
            }
            else
            {
                Debug.WriteLine($"[LoadUser] Status: {response.StatusCode}");
                WelcomeLabel.Text = "Witaj";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd w LoadUser: {ex}");
            WelcomeLabel.Text = "Witaj";
        }
    }

    // Animacja kliknięcia i przejście do wybranej strony
    private async Task AnimateAndNavigate(VisualElement sender, string route)
    {
        await sender.ScaleTo(0.95, 100);
        await sender.ScaleTo(1.0, 100);
        await Shell.Current.GoToAsync(route);
    }

    // Przejście do mapy
    private async void OnMapClicked(object sender, EventArgs e)
    {
        await AnimateAndNavigate((VisualElement)sender, nameof(MapPage));
    }

    // Przejście do listy wydarzeń
    private async void OnEventsClicked(object sender, EventArgs e)
    {
        await AnimateAndNavigate((VisualElement)sender, nameof(EventListPage));
    }

    // Przejście do dodawania wydarzenia
    private async void OnAddClicked(object sender, EventArgs e)
    {
        await AnimateAndNavigate((VisualElement)sender, nameof(AddEventPage));
    }

    // Przejście do profilu użytkownika
    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await AnimateAndNavigate((VisualElement)sender, nameof(ProfilePage));
    }

    // Wylogowywanie i czyszczenie zapisanych danych
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert("Wyloguj", "Czy na pewno chcesz się wylogować?", "Tak", "Anuluj");
        if (!confirm) return;

        Debug.WriteLine("Wylogowywanie...");

        // Logujemy dane przed czyszczeniem
        Debug.WriteLine("Token przed: " + Preferences.Get("auth_token", "brak"));
        Debug.WriteLine("UserId przed: " + Preferences.Get("user_id", "brak"));
        Debug.WriteLine("Lat przed: " + Preferences.Get("latitude", "brak"));
        Debug.WriteLine("Lon przed: " + Preferences.Get("longitude", "brak"));

        Preferences.Clear(); // Czyścimy wszystkie dane

        // Potwierdzenie wyczyszczenia
        Debug.WriteLine("Token po: " + Preferences.Get("auth_token", "brak"));
        Debug.WriteLine("UserId po: " + Preferences.Get("user_id", "brak"));

        await Shell.Current.GoToAsync($"//LoginPage");
    }
}
