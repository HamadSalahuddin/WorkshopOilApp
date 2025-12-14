using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkshopOilApp.Services;
using WorkshopOilApp.Views;
using WorkshopOilApp;

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
            AuthService.SetCurrentUser(result.Data);
            if (Shell.Current is AppShell appShell)
            {
                appShell.SetAuthenticatedState();
            }
            // Navigate to Customers List
            await Shell.Current.GoToAsync($"//{nameof(CustomerListPage)}");
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

    [RelayCommand]
    async Task BackupDatabase()
    {
        try
        {
            // Database ka original path lein
            string asliPath = GetDatabasePath();

            // Check karen ke database file hai ke nahi
            if (!File.Exists(asliPath))
            {
                await Shell.Current.DisplayAlert("Error", "Database file nahi mili!", "OK");
                return;
            }

#if ANDROID
        // Android phone ke liye
        string downloadsFolder = Path.Combine(
            Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, 
            Android.OS.Environment.DirectoryDownloads
        );
        
        string nayiPath = Path.Combine(downloadsFolder, "copied_database.db3");
        
        // File copy karen
        File.Copy(asliPath, nayiPath, true);
        
        await Shell.Current.DisplayAlert("Success", 
            $"Database copy ho gayi!\nLocation: {nayiPath}", "OK");

#elif WINDOWS
        // Windows computer ke liye
        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
        string nayiPath = Path.Combine(downloadsFolder, "copied_database.db3");
        
        File.Copy(asliPath, nayiPath, true);
        await Shell.Current.DisplayAlert("Success", "Database Downloads folder mein copy ho gayi", "OK");

#elif IOS
        // iPhone ke liye - Share option use karen
        //await ShareFile(asliPath);
#endif
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Copy nahi ho payi: {ex.Message}", "OK");
        }
    }

    public string GetDatabasePath()
    {
        string databaseName = "WorkshopDb.db3";
        string databasePath = string.Empty;

#if ANDROID
    databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName);
#elif IOS
    databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", databaseName);
#elif WINDOWS
    // Windows ke liye
    databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName);
#endif

        return databasePath;
    }

    // iOS ke liye file share karen
    public async Task ShareFile(string filePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Database File",
            File = new ShareFile(filePath)
        });
    }
}