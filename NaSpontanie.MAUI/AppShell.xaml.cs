namespace NaSpontanie.MAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(MapPage), typeof(MapPage));
            Routing.RegisterRoute(nameof(EventListPage), typeof(EventListPage));
            Routing.RegisterRoute(nameof(AddEventPage), typeof(AddEventPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            Routing.RegisterRoute(nameof(EventDetailPage), typeof(EventDetailPage));

            GoToAsync($"//{nameof(LoginPage)}");
        }
    }
}
