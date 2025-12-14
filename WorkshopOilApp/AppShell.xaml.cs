using System.Linq;
using System.Threading.Tasks;
using WorkshopOilApp.Services;

namespace WorkshopOilApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            SetLoggedOutState();
        }

        public void SetAuthenticatedState()
        {
            FlyoutBehavior = FlyoutBehavior.Flyout;
        }

        public void SetLoggedOutState()
        {
            FlyoutBehavior = FlyoutBehavior.Disabled;
            Shell.Current.CurrentItem = Items.First();
        }

        public async Task LogoutAsync()
        {
            AuthService.Logout();
            SetLoggedOutState();
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await LogoutAsync();
        }
    }
}
