using Microsoft.Maui.ApplicationModel;
using WorkshopOilApp.ViewModels;
using WorkshopOilApp.Services;
using WorkshopOilApp;

namespace WorkshopOilApp.Views;

public partial class CustomerListPage : ContentPage
{
    private DateTime _lastSearchTime;
    private const int SearchDelayMs = 600; // 0.6 seconds

    public CustomerListPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CustomerListViewModel vm)
        {
            vm.OnAppearing();
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var now = DateTime.Now;
        _lastSearchTime = now;

        await Task.Delay(SearchDelayMs);

        // Only execute if no new typing happened
        if ((DateTime.Now - _lastSearchTime).TotalMilliseconds >= SearchDelayMs - 10)
        {
            if (BindingContext is CustomerListViewModel vm && !vm.IsBusy)
            {
                await vm.LoadCustomersCommand.ExecuteAsync(null);
            }
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            return base.OnBackButtonPressed();
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var confirm = await DisplayAlert("Logout", "Do you want to logout?", "Yes", "No");
            if (confirm && Shell.Current is AppShell appShell)
            {
                await appShell.LogoutAsync();
            }
        });

        return true;
    }
}
