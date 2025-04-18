using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NaSpontanie.MAUI.Models;

namespace NaSpontanie.MAUI;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();

        LoadUserData();  // Wczytuje dane aktualnego użytkownika
        LoadFriends();   // Wczytuje listę znajomych
    }

    // Pobiera dane użytkownika i wyświetla je w formularzu
    private async void LoadUserData()
    {
        var token = Preferences.Get("auth_token", null);
        var userId = Preferences.Get("user_id", null);

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
            return;

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.18.10:5206")
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"api/users/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (user != null)
                {
                    UsernameEntry.Text = user.Username;
                    EmailEntry.Text = user.Email;
                    BioEditor.Text = user.Bio;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się pobrać danych użytkownika: {ex.Message}", "OK");
        }
    }

    // Pobiera znajomych z API i przypisuje do widoku
    private async void LoadFriends()
    {
        var userId = Preferences.Get("user_id", null);
        var token = Preferences.Get("auth_token", null);

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token)) return;

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"api/friendships/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var friends = JsonSerializer.Deserialize<List<UserModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                FriendsList.ItemsSource = friends;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się pobrać znajomych: {ex.Message}", "OK");
        }
    }

    // Zapisuje zmienione dane użytkownika
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var token = Preferences.Get("auth_token", null);
        var userId = Preferences.Get("user_id", null);

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
            return;

        if (!int.TryParse(userId, out int parsedUserId))
        {
            await DisplayAlert("Błąd", "Nieprawidłowy identyfikator użytkownika.", "OK");
            return;
        }

        var updatedUser = new UserModel
        {
            Id = parsedUserId,
            Username = UsernameEntry.Text,
            Email = EmailEntry.Text,
            Bio = BioEditor.Text
        };

        var json = JsonSerializer.Serialize(updatedUser);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.18.10:5206")
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PutAsync($"api/users/{userId}", content);
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                await DisplayAlert("Sukces", "Dane zaktualizowane", "OK");
            }
            else
            {
                await DisplayAlert("Błąd", "Nie udało się zapisać zmian", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się zapisać zmian: {ex.Message}", "OK");
        }
    }

    // Obsługuje zmianę hasła użytkownika
    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        var oldPassword = OldPasswordEntry.Text;
        var newPassword = NewPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            await DisplayAlert("Błąd", "Wprowadź stare i nowe hasło.", "OK");
            return;
        }

        var token = Preferences.Get("auth_token", null);
        if (string.IsNullOrWhiteSpace(token))
        {
            await DisplayAlert("Błąd", "Brak tokenu. Zaloguj się ponownie.", "OK");
            return;
        }

        var changeRequest = new
        {
            OldPassword = oldPassword,
            NewPassword = newPassword
        };

        var json = JsonSerializer.Serialize(changeRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://192.168.18.10:5206")
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync("api/auth/change-password", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sukces", "Hasło zostało zmienione.", "OK");
                OldPasswordEntry.Text = "";
                NewPasswordEntry.Text = "";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Błąd", $"Nie udało się zmienić hasła: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Błąd serwera: {ex.Message}", "OK");
        }
    }

    // Obsługuje usunięcie znajomego z listy
    private async void OnDeleteFriendTapped(object sender, EventArgs e)
    {
        if (sender is Label label &&
            label.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap &&
            tap.CommandParameter is int friendId)
        {
            var confirm = await DisplayAlert("Usuń znajomego", "Czy na pewno chcesz usunąć tego znajomego?", "Tak", "Anuluj");
            if (!confirm) return;

            var token = Preferences.Get("auth_token", null);
            var userId = Preferences.Get("user_id", null);

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
            {
                await DisplayAlert("Błąd", "Brak danych użytkownika.", "OK");
                return;
            }

            try
            {
                using var client = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"api/friendships?userId={userId}&friendId={friendId}");

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sukces", "Znajomy został usunięty.", "OK");
                    LoadFriends(); // Odśwież widok listy
                }
                else
                {
                    await DisplayAlert("Błąd", "Nie udało się usunąć znajomego.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Wystąpił błąd: {ex.Message}", "OK");
            }
        }
    }
}
