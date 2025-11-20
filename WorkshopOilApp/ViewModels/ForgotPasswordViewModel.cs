// ViewModels/ForgotPasswordViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkshopOilApp.Services;

namespace WorkshopOilApp.ViewModels;

public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly AuthService _authService = new();

    [ObservableProperty] string username = "";
    [ObservableProperty] string passcode = "";
    [ObservableProperty] string newPassword = "";
    [ObservableProperty] string confirmPassword = "";
    [ObservableProperty] string errorMessage = "";
    [ObservableProperty] bool hasError;
    [ObservableProperty] bool isBusy;

    [ObservableProperty] bool showNewPasswordFields;
    [ObservableProperty] bool showSuccessMessage;

    [ObservableProperty] string submitButtonText = "Verify Username & Passcode";

    [RelayCommand]
    async Task Submit()
    {
        HasError = false;
        ErrorMessage = "";
        ShowSuccessMessage = false;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Please enter your username";
            HasError = true;
            return;
        }

        if (!ShowNewPasswordFields && string.IsNullOrWhiteSpace(Passcode))
        {
            ErrorMessage = "Please enter your recovery passcode";
            HasError = true;
            return;
        }

        if (ShowNewPasswordFields)
        {
            // Second step: Change password
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                HasError = true;
                return;
            }
            if (NewPassword.Length < 6)
            {
                ErrorMessage = "Password must be at least 6 characters";
                HasError = true;
                return;
            }

            IsBusy = true;
            var result = await _authService.ResetPasswordAsync(Username, Passcode, NewPassword);
            IsBusy = false;

            if (result.IsSuccess)
            {
                ShowSuccessMessage = true;
                SubmitButtonText = "Done";
                await Task.Delay(2000);
                await Shell.Current.GoToAsync(".."); // back to login
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
                HasError = true;
            }
        }
        else
        {
            // First step: Verify username + passcode only (no password yet)
            IsBusy = true;
            var tempResult = await _authService.ResetPasswordAsync(Username, Passcode, "temp_check_only");
            IsBusy = false;

            if (tempResult.IsSuccess)
            {
                ShowNewPasswordFields = true;
                SubmitButtonText = "Change Password";
                ErrorMessage = "";
            }
            else
            {
                ErrorMessage = "Invalid username or recovery passcode";
                HasError = true;
            }
        }
    }

    [RelayCommand]
    async Task GoBackToLogin() => await Shell.Current.GoToAsync("//LoginPage");
}