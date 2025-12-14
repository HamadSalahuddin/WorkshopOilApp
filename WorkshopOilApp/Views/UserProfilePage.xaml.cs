using WorkshopOilApp.ViewModels;

namespace WorkshopOilApp.Views;

public partial class UserProfilePage : ContentPage
{
    public UserProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is UserProfileViewModel vm)
        {
            await vm.LoadUserAsync();
        }
    }
}
