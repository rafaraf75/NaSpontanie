using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using NaSpontanie.MAUI.Services;

namespace NaSpontanie.MAUI;

public partial class LoginPage : ContentPage
{
    private HttpClient _httpClient;

    public LoginPage()
    {
        InitializeComponent();
        _httpClient = ApiService.CreateClient(); // Inicjalizacja klienta HTTP
    }

    // Logowanie użytkownika
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Błąd", "Wprowadź email i hasło", "OK");
            return;
        }

        var payload = new { Email = email, Password = password };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            Debug.WriteLine("Start logowania");
            var response = await _httpClient.PostAsync("api/auth/login", content);
            Debug.WriteLine("Po response");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Błąd", "Nieprawidłowe dane logowania", "OK");
                return;
            }

            var result = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Token (surowy JSON): {result}");

            string? token = null;

            try
            {
                var tokenJson = JsonSerializer.Deserialize<JsonElement>(result);
                if (tokenJson.TryGetProperty("token", out var tokenElement))
                {
                    token = tokenElement.GetString();
                }
                else
                {
                    Debug.WriteLine("Brak pola 'token' w odpowiedzi!");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd parsowania tokena: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                await DisplayAlert("Błąd", "Nie udało się odczytać tokena", "OK");
                return;
            }

            Preferences.Set("auth_token", token);

            _httpClient = ApiService.CreateClient(); // Odświeżenie klienta z tokenem
            Debug.WriteLine($"[LOGIN] Token: {token}");
            Debug.WriteLine($"[LOGIN] Headers: {_httpClient.DefaultRequestHeaders.Authorization}");

            var userResponse = await _httpClient.GetAsync("api/users/me");

            if (!userResponse.IsSuccessStatusCode)
            {
                await DisplayAlert("Błąd", "Nie udało się pobrać danych użytkownika.", "OK");
                return;
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            Debug.WriteLine($"User JSON: {userJson}");
            var user = JsonSerializer.Deserialize<JsonElement>(userJson);

            if (!user.TryGetProperty("id", out var idProp))
            {
                Debug.WriteLine("Brak pola 'id' w odpowiedzi API!");
                await DisplayAlert("Błąd", "Brak identyfikatora użytkownika w odpowiedzi.", "OK");
                return;
            }

            var userId = idProp.GetInt32().ToString();
            Preferences.Set("user_id", userId);

            // Sprawdzenie i zapis lokalizacji
            if (user.TryGetProperty("latitude", out var latProp) &&
                user.TryGetProperty("longitude", out var lonProp) &&
                latProp.ValueKind == JsonValueKind.Number &&
                lonProp.ValueKind == JsonValueKind.Number)
            {
                var latitude = latProp.GetDouble();
                var longitude = lonProp.GetDouble();

                if (latitude != 0 && longitude != 0)
                {
                    Preferences.Set("latitude", latitude);
                    Preferences.Set("longitude", longitude);
                    Debug.WriteLine($"Lokalizacja z bazy: {latitude}, {longitude}");
                }
                else
                {
                    Debug.WriteLine("Lokalizacja to (0,0) – pobieranie aktualnej...");
                    await RequestAndSaveLocation(userId, token);
                }
            }
            else
            {
                Debug.WriteLine("Brak lokalizacji – pobieranie aktualnej...");
                await RequestAndSaveLocation(userId, token);
            }

            await DisplayAlert("Sukces", "Zalogowano!", "OK");

            if (Shell.Current?.Items != null)
            {
                var startTab = Shell.Current.Items.FirstOrDefault(i => i.Title == "Start");
                if (startTab != null)
                {
                    Shell.Current.CurrentItem = startTab;
                }
                else
                {
                    await DisplayAlert("Błąd", "Nie można przełączyć na zakładkę Start.", "OK");
                }
            }
            else
            {
                Debug.WriteLine("Shell.Current lub Items są null – UI jeszcze się nie zainicjalizowało?");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Login error: {ex}");
            await DisplayAlert("Błąd", $"Coś poszło nie tak: {ex.Message}", "OK");
        }
    }

    // Pobiera aktualną lokalizację i zapisuje ją do serwera i ustawień
    private async Task RequestAndSaveLocation(string userId, string token)
    {
        await DisplayAlert("Lokalizacja", "Potrzebujemy Twojej lokalizacji, aby pokazywać wydarzenia w pobliżu.", "OK");

        var location = await Geolocation.GetLastKnownLocationAsync();
        if (location == null)
            location = await Geolocation.GetLocationAsync();

        if (location != null)
        {
            Preferences.Set("latitude", location.Latitude);
            Preferences.Set("longitude", location.Longitude);

            var body = new
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/users/{userId}/location")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _httpClient.SendAsync(request);
        }
    }

    // Przenosi użytkownika do strony rejestracji
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    // Pomocnicza metoda do dekodowania userId z tokena JWT
    private string? GetUserIdFromToken(string token)
    {
        try
        {
            var payload = token.Split('.')[1];
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(AddPadding(payload)));
            var payloadData = JsonDocument.Parse(json);

            return payloadData.RootElement.TryGetProperty("sub", out var sub)
                ? sub.GetString()
                : null;
        }
        catch
        {
            return null;
        }
    }

    // Uzupełnia padding base64, jeśli jest brakujący
    private static string AddPadding(string base64)
    {
        return (base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            0 => base64,
            _ => throw new FormatException("Niepoprawny base64.")
        };
    }
}
