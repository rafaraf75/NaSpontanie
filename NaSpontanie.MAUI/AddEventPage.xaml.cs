using System.Text;
using System.Text.Json;
using NaSpontanie.MAUI.Models;
using NaSpontanie.MAUI.Services;

namespace NaSpontanie.MAUI;

public partial class AddEventPage : ContentPage
{
    private HttpClient _httpClient => ApiService.CreateClient();
    private List<InterestModel> _interests = new();

    public AddEventPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInterests();       // Załaduj dostępne kategorie  
        await LoadUserEvents();      // Załaduj wydarzenia użytkownika
    }

    // Pobiera listę kategorii (zainteresowań) z API
    private async Task LoadInterests()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/interests");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Błąd", "Nie udało się pobrać kategorii.", "OK");
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            _interests = JsonSerializer.Deserialize<List<InterestModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<InterestModel>();

            InterestPicker.ItemsSource = _interests;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować kategorii: {ex.Message}", "OK");
        }
    }

    // Pobiera wydarzenia stworzone przez zalogowanego użytkownika
    private async Task LoadUserEvents()
    {
        var userId = Preferences.Get("user_id", null);
        if (string.IsNullOrEmpty(userId))
            return;

        try
        {
            var response = await _httpClient.GetAsync("api/events");

            if (!response.IsSuccessStatusCode)
                return;

            var json = await response.Content.ReadAsStringAsync();
            var allEvents = JsonSerializer.Deserialize<List<EventModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var userEvents = allEvents?
                .Where(e => e.Creator != null && e.Creator.Id.ToString() == userId)
                .ToList();

            MyEventsList.ItemsSource = userEvents;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować wydarzeń: {ex.Message}", "OK");
        }
    }

    // Czyści formularz po dodaniu wydarzenia
    private void ClearForm()
    {
        TitleEntry.Text = string.Empty;
        DescriptionEditor.Text = string.Empty;
        EventDatePicker.Date = DateTime.Now;
        InterestPicker.SelectedItem = null;
    }

    // Obsługuje usunięcie wybranego wydarzenia
    private async void OnDeleteTapped(object sender, EventArgs e)
    {
        if (sender is Label label && label.BindingContext is EventModel selectedEvent)
        {
            var confirm = await DisplayAlert("Usuń wydarzenie", $"Czy na pewno chcesz usunąć „{selectedEvent.Title}”?", "Tak", "Anuluj");
            if (!confirm) return;

            try
            {
                var response = await _httpClient.DeleteAsync($"api/events/{selectedEvent.Id}");
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Sukces", "Wydarzenie usunięte.", "OK");
                    await LoadUserEvents();
                }
                else
                {
                    await DisplayAlert("Błąd", "Nie udało się usunąć wydarzenia.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Błąd podczas usuwania: {ex.Message}", "OK");
            }
        }
    }

    // Obsługuje dodanie nowego wydarzenia
    private async void OnAddEventClicked(object sender, EventArgs e)
    {
        try
        {
            var latitude = Preferences.Get("latitude", 0.0);
            var longitude = Preferences.Get("longitude", 0.0);

            // Walidacja pól formularza
            if (string.IsNullOrWhiteSpace(TitleEntry.Text) || string.IsNullOrWhiteSpace(DescriptionEditor.Text) || InterestPicker.SelectedItem == null)
            {
                await DisplayAlert("Błąd", "Uzupełnij wszystkie pola, w tym kategorię!", "OK");
                return;
            }

            var selectedInterest = (InterestModel)InterestPicker.SelectedItem;

            var newEvent = new
            {
                title = TitleEntry.Text,
                description = DescriptionEditor.Text,
                date = EventDatePicker.Date,
                latitude,
                longitude,
                interestIds = new List<int> { selectedInterest.Id }
            };

            var json = JsonSerializer.Serialize(newEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Events", content);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Sukces", "Wydarzenie dodane!", "OK");
                ClearForm();
                await LoadUserEvents();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await DisplayAlert("Błąd", $"Nie udało się dodać wydarzenia: {error}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Wystąpił błąd: {ex.Message}", "OK");
        }
    }
}
