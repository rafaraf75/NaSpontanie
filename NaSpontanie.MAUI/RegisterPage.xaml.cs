using System.Text;
using System.Text.Json;
using System.Diagnostics;
using NaSpontanie.MAUI.Services;

namespace NaSpontanie.MAUI;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        // Pobranie danych z formularza
        var username = UsernameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // Walidacja pól
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Błąd", "Uzupełnij wszystkie pola.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Błąd", "Hasła się nie zgadzają.", "OK");
            return;
        }

        // Tymczasowe współrzędne – aktualizowane po logowaniu
        var payload = new
        {
            Username = username,
            Email = email,
            Password = password,
            Latitude = 0.0,
            Longitude = 0.0
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            // Inicjalizacja klienta HTTP
            var client = ApiService.CreateClient();
            if (client == null)
            {
                await DisplayAlert("Błąd", "Brak klienta HTTP.", "OK");
                return;
            }

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Wysłanie żądania rejestracji
            var response = await client.PostAsync("api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sukces", "Zarejestrowano!", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            else
            {
                // Obsługa błędu z API
                var error = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[Register Error] {error}");
                await DisplayAlert("Błąd", error, "OK");
            }
        }
        catch (Exception ex)
        {
            // Obsługa wyjątków HTTP lub JSON
            Debug.WriteLine($"[Register Exception] {ex}");
            await DisplayAlert("Błąd", ex.Message, "OK");
        }
    }

    // Nawigacja z powrotem do ekranu logowania
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }
}
