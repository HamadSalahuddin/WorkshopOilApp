namespace WorkshopOilApp.Views;

public partial class AddEditCustomerPage : ContentPage
{
    public AddEditCustomerPage()
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