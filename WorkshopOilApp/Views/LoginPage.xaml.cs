using WorkshopOilApp.Services;
using WorkshopOilApp;

namespace WorkshopOilApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Shell.Current is AppShell appShell)
        {
            appShell.SetLoggedOutState();
        }
    }
}