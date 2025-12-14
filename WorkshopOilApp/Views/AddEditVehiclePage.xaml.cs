namespace WorkshopOilApp.Views;

public partial class AddEditVehiclePage : ContentPage
{
    public AddEditVehiclePage()
    {
        InitializeComponent();
        ConfigureBackButton();
    }

    private void ConfigureBackButton()
    {
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = new Command(async () => await Shell.Current.GoToAsync(".."))
        });
    }
}