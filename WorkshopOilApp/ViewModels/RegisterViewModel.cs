// ViewModels/RegisterViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;

public partial class RegisterViewModel : ObservableObject
{
    // All [ObservableProperty] fields...
    [ObservableProperty] string givenName = "";
    [ObservableProperty] string lastName = "";
    [ObservableProperty] string businessName = "";
    [ObservableProperty] string username = "";
    [ObservableProperty] string passCode = "";
    [ObservableProperty] string password = "";
    [ObservableProperty] string confirmPassword = "";
    [ObservableProperty] string businessContact = "";
    [ObservableProperty] string businessEmail = "";
    [ObservableProperty] string errorMessage = "";
    [ObservableProperty] bool hasError;
    [ObservableProperty] bool isBusy;

    private readonly AuthService _authService = new();

    [RelayCommand]
    async Task Register()
    {
        HasError = false;
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match";
            HasError = true;
            return;
        }

        IsBusy = true;
        var user = new User
        {
            GivenName = GivenName,
            LastName = LastName,
            BusinessName = BusinessName,
            UserName = Username,
            PassCode = PassCode,
            BusinessContact = BusinessContact,
            BusinessEmail = BusinessEmail
        };

        var result = await _authService.RegisterAsync(user, Password);
        IsBusy = false;

        if (result.IsSuccess)
            await Shell.Current.GoToAsync(".."); // back to login
        else
        {
            ErrorMessage = result.ErrorMessage;
            HasError = true;
        }
    }
}