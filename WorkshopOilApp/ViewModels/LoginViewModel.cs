// ViewModels/LoginViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkshopOilApp.Services;
using WorkshopOilApp.Views;

namespace WorkshopOilApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty] string username = "";
    [ObservableProperty] string password = "";
    [ObservableProperty] string errorMessage = "";
    [ObservableProperty] bool isBusy;
    [ObservableProperty] bool hasError;

    public LoginViewModel()
    {
        _authService = new AuthService();
    }

    [RelayCommand]
    async Task Login()
    {
        HasError = false;
        ErrorMessage = "";
        IsBusy = true;

        var result = await _authService.LoginAsync(Username, Password);

        if (result.IsSuccess)
        {
            // Navigate to Customers List
            //await Shell.Current.GoToAsync($"//{nameof(CustomerListPage)}");
        }
        else
        {
            ErrorMessage = result.ErrorMessage;
            HasError = true;
        }

        IsBusy = false;
    }

    [RelayCommand]
    async Task GoToRegister() => await Shell.Current.GoToAsync(nameof(RegisterPage));

    [RelayCommand]
    async Task GoToForgotPassword() => await Shell.Current.GoToAsync(nameof(ForgotPasswordPage));
}