using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using WorkshopOilApp.Services;
using WorkshopOilApp.Services.Repositories;

namespace WorkshopOilApp.ViewModels;

public partial class UserProfileViewModel : ObservableObject
{
    private readonly UserRepository _userRepository = new();

    [ObservableProperty] private string givenName = string.Empty;
    [ObservableProperty] private string lastName = string.Empty;
    [ObservableProperty] private string userName = string.Empty;
    [ObservableProperty] private string passCode = string.Empty;
    [ObservableProperty] private string businessName = string.Empty;
    [ObservableProperty] private string businessContact = string.Empty;
    [ObservableProperty] private string? businessEmail = string.Empty;

    public async Task LoadUserAsync()
    {
        if (AuthService.CurrentUser is null)
        {
            await Shell.Current.DisplayAlert("Not logged in", "Please login again to view your profile.", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        var user = AuthService.CurrentUser;
        GivenName = user.GivenName;
        LastName = user.LastName;
        UserName = user.UserName;
        PassCode = user.PassCode;
        BusinessName = user.BusinessName;
        BusinessContact = user.BusinessContact;
        BusinessEmail = user.BusinessEmail;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (AuthService.CurrentUser is null)
        {
            await Shell.Current.DisplayAlert("Not logged in", "Please login again.", "OK");
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        var user = AuthService.CurrentUser;
        user.GivenName = GivenName;
        user.LastName = LastName;
        user.BusinessName = BusinessName;
        user.BusinessContact = BusinessContact;
        user.BusinessEmail = BusinessEmail;
        user.UpdatedAt = DateTime.UtcNow.ToString("o");

        var updateResult = await _userRepository.UpdateAsync(user);
        if (!updateResult.IsSuccess)
        {
            await Shell.Current.DisplayAlert("Error", updateResult.ErrorMessage ?? "Unable to update profile.", "OK");
            return;
        }

        AuthService.SetCurrentUser(user);
        await Shell.Current.DisplayAlert("Success", "Profile updated successfully.", "OK");
    }
}
