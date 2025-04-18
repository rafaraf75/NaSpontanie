using System.Diagnostics;

namespace NaSpontanie.MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Debug.WriteLine(">>> App starting...");

            var token = Preferences.Get("auth_token", null);

            if (!string.IsNullOrEmpty(token))
            {
                MainPage = new AppShell();
            }
            else
            {
                MainPage = new NavigationPage(new LoginPage());
            }
        }
    }
}

