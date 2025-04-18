using System.Net.Http.Headers;

namespace NaSpontanie.MAUI.Services;

public static class ApiService
{
    private static string BaseUrl = "http://192.168.18.10:5206";

    public static HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        var token = Preferences.Get("auth_token", null);
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }
}
