using NaSpontanie.MAUI.Dtos;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace NaSpontanie.MAUI;

[QueryProperty(nameof(SelectedEvent), "SelectedEvent")]
public partial class EventDetailPage : ContentPage
{
    private bool _isSendButtonHooked = false;
    public EventDto? SelectedEvent { get; set; }

    public EventDetailPage()
    {
        InitializeComponent();
        AddFriendIcon.TextColor = Color.FromArgb("#2b0098");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Przypięcie obsługi kliknięcia przycisku tylko raz
        if (!_isSendButtonHooked)
        {
            SendButton.Clicked += OnSendCommentClicked;
            _isSendButtonHooked = true;
        }

        if (SelectedEvent != null)
        {
            // Oblicz dystans do wydarzenia
            var userLat = Preferences.Get("latitude", 0.0);
            var userLon = Preferences.Get("longitude", 0.0);
            SelectedEvent.DistanceKm = CalculateDistance(userLat, userLon, SelectedEvent.Latitude, SelectedEvent.Longitude);

            var currentUserId = Preferences.Get("user_id", null);
            // Ukryj ikonę dołączenia, jeśli to wydarzenie użytkownika
            if (currentUserId == SelectedEvent.Creator?.Id.ToString())
            {
                JoinRequestIcon.IsVisible = false;
            }
            else
            {
                CheckJoinAvailability();
            }

            LoadEventDetails();
            _ = LoadComments();
        }
    }

    // Uzupełnia UI danymi o wydarzeniu
    private async void LoadEventDetails()
    {
        if (SelectedEvent == null) return;

        TitleLabel.Text = SelectedEvent.Title;
        DescriptionLabel.Text = SelectedEvent.Description;
        DateLabel.Text = $" {SelectedEvent.Date:dd MMMM yyyy HH:mm}";

        if (SelectedEvent.Creator != null)
            CreatorLabel.Text = $" Twórca: {SelectedEvent.Creator.Username}";

        var userLat = Preferences.Get("latitude", 0.0);
        var userLon = Preferences.Get("longitude", 0.0);

        var dist = CalculateDistance(userLat, userLon, SelectedEvent.Latitude, SelectedEvent.Longitude);
        DistanceLabel.Text = $"Odległość: {dist:0.00} km";

        CategoryLabel.Text = $"Kategoria: {SelectedEvent.InterestName}";

        var currentUserId = Preferences.Get("user_id", null);
        if (currentUserId != null && SelectedEvent.Creator != null)
        {
            // Sprawdź czy użytkownik ma już tego twórcę w znajomych
            if (SelectedEvent.Creator.Id.ToString() == currentUserId)
            {
                AddFriendIcon.IsVisible = false;
            }
            else
            {
                await CheckFriendshipAsync(int.Parse(currentUserId), SelectedEvent.Creator.Id);
            }
        }
    }

    // Sprawdzenie czy twórca wydarzenia jest już znajomym
    private async Task CheckFriendshipAsync(int userId, int friendId)
    {
        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            var token = Preferences.Get("auth_token", null);

            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"api/friendships/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var friends = JsonSerializer.Deserialize<List<UserDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var isAlreadyFriend = friends?.Any(f => f.Id == friendId) == true;

                if (isAlreadyFriend)
                {
                    AddFriendIcon.Text = "\uf4fc";
                    AddFriendIcon.TextColor = Color.FromArgb("#28a745");
                    AddFriendIcon.GestureRecognizers.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CheckFriendship] Błąd: {ex.Message}");
        }
    }

    // Pobiera komentarze przypisane do wydarzenia
    private async Task LoadComments()
    {
        if (SelectedEvent == null) return;

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            var token = Preferences.Get("auth_token", null);
            var currentUserId = Preferences.Get("user_id", "0");

            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"/api/Comments?eventId={SelectedEvent.Id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var comments = JsonSerializer.Deserialize<List<CommentDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var comment in comments!)
                {
                    comment.UserIdAsString = comment.UserId.ToString();
                    comment.CanDelete = comment.UserId.ToString() == currentUserId;
                    comment.CanReport = comment.UserId.ToString() != currentUserId;
                }

                CommentsList.ItemsSource = comments;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd przy ładowaniu komentarzy: {ex}");
        }
    }

    // Obsługa wysyłania nowego komentarza
    private async void OnSendCommentClicked(object? sender, EventArgs e)
    {
        if (SelectedEvent == null) return;

        var text = CommentEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        var token = Preferences.Get("auth_token", null);
        if (string.IsNullOrEmpty(token)) return;

        var userId = Preferences.Get("user_id", "0");
        var comment = new CreateCommentDto
        {
            EventId = SelectedEvent.Id,
            Text = text,
            UserId = int.Parse(userId)
        };

        var json = JsonSerializer.Serialize(comment);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync("/api/Comments", httpContent);
            if (response.IsSuccessStatusCode)
            {
                CommentEntry.Text = string.Empty;
                await LoadComments();
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Błąd dodania komentarza: {err}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Wyjątek przy dodawaniu komentarza: {ex}");
        }
    }

    // Usuwanie komentarza przez autora
    private async void OnDeleteCommentTapped(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var gesture = label.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
            if (gesture?.CommandParameter is int commentId)
            {
                var confirm = await DisplayAlert("Usuń komentarz", "Czy na pewno chcesz usunąć ten komentarz?", "Tak", "Anuluj");
                if (!confirm) return;

                var token = Preferences.Get("auth_token", null);
                if (string.IsNullOrEmpty(token))
                {
                    await DisplayAlert("Błąd", "Brak tokena autoryzacji.", "OK");
                    return;
                }

                try
                {
                    using var client = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = await client.DeleteAsync($"api/comments/{commentId}");
                    if (response.IsSuccessStatusCode)
                    {
                        await LoadComments();
                        await DisplayAlert("Sukces", "Komentarz usunięty.", "OK");
                    }
                    else
                    {
                        var errorText = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Błąd", $"Nie udało się usunąć komentarza.\nKod: {(int)response.StatusCode}\nTreść: {errorText}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Błąd", $"Wystąpił problem: {ex.Message}", "OK");
                }
            }
        }
    }

    // Zgłoszenie komentarza przez innego użytkownika
    private async void OnReportCommentTapped(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var gesture = label.GestureRecognizers.FirstOrDefault() as TapGestureRecognizer;
            if (gesture?.CommandParameter is int commentId)
            {
                var confirm = await DisplayAlert("Zgłoś komentarz", "Czy na pewno chcesz zgłosić ten komentarz do moderacji?", "Tak", "Anuluj");
                if (!confirm) return;

                await SendReportAsync(commentId);
            }
        }
    }

    private async Task SendReportAsync(int commentId)
    {
        var token = Preferences.Get("auth_token", null);
        var userId = Preferences.Get("user_id", null);

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId) || SelectedEvent == null)
        {
            await DisplayAlert("Błąd", "Brakuje danych logowania lub wydarzenia.", "OK");
            return;
        }

        var report = new CreateReportDto
        {
            ReporterId = int.Parse(userId),
            ReportedEventId = SelectedEvent.Id
        };

        var json = JsonSerializer.Serialize(report);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("api/reports", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Dzięki", "Komentarz został zgłoszony do moderacji.", "OK");
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Błąd", $"Nie udało się zgłosić komentarza: {err}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Coś poszło nie tak: {ex.Message}", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // Dodawanie twórcy wydarzenia do znajomych
    private async void OnAddFriendClicked(object sender, EventArgs e)
    {
        if (SelectedEvent?.Creator == null)
            return;

        var currentUserId = Preferences.Get("user_id", null);
        if (string.IsNullOrEmpty(currentUserId))
        {
            await DisplayAlert("Błąd", "Nie jesteś zalogowany.", "OK");
            return;
        }

        var friendship = new FriendshipDto
        {
            UserId = int.Parse(currentUserId),
            FriendId = SelectedEvent.Creator.Id
        };

        var json = JsonSerializer.Serialize(friendship);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var httpClient = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            var token = Preferences.Get("auth_token", null);

            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync("api/friendships", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sukces", "Dodano do znajomych!", "OK");
                AddFriendIcon.Text = "\uf4fc";
                AddFriendIcon.TextColor = Color.FromArgb("#28a745");
                AddFriendIcon.GestureRecognizers.Clear();
            }
            else
            {
                await DisplayAlert("Błąd", "Nie udało się dodać do znajomych.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Wystąpił problem: {ex.Message}", "OK");
        }
    }

    private bool _hasJoined = false;

    // Obsługa kliknięcia przycisku dołączenia do wydarzenia
    private async void OnJoinToggleClicked(object sender, EventArgs e)
    {
        _hasJoined = !_hasJoined;

        JoinRequestIcon.TextColor = _hasJoined
            ? Color.FromArgb("#28a745") 
            : Color.FromArgb("#2b0098");
        if (_hasJoined)
            await SendJoinRequestAsync();
    }

    private async Task SendJoinRequestAsync()
    {
        if (SelectedEvent == null) return;

        var userId = Preferences.Get("user_id", null);
        var token = Preferences.Get("auth_token", null);

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            await DisplayAlert("Błąd", "Brak danych logowania.", "OK");
            return;
        }

        var joinRequest = new
        {
            UserId = int.Parse(userId),
            EventId = SelectedEvent.Id
        };

        var json = JsonSerializer.Serialize(joinRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PostAsync("/api/joinrequests", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sukces", "Dołączono do wydarzenia!", "OK");
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Błąd", $"Nie udało się dołączyć: {err}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Wystąpił problem: {ex.Message}", "OK");
        }
    }

    // Sprawdza czy użytkownik już dołączył do wydarzenia
    private async void CheckJoinAvailability()
    {
        _hasJoined = false;

        var userId = Preferences.Get("user_id", null);
        var token = Preferences.Get("auth_token", null);

        if (SelectedEvent == null || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return;

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://192.168.18.10:5206") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/api/joinrequests/exists?userId={userId}&eventId={SelectedEvent.Id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _hasJoined = bool.Parse(json);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CheckJoinAvailability] Błąd: {ex.Message}");
        }

        JoinRequestIcon.Text = "\uf055";
        JoinRequestIcon.TextColor = _hasJoined ? Color.FromArgb("#28a745") : Color.FromArgb("#2b0098");
    }

    // Oblicza dystans między dwoma punktami na mapie
    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(R * c, 2);
    }
}
