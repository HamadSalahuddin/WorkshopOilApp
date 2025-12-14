namespace WorkshopOilApp.Views;

public partial class AddOilChangePage : ContentPage
{
    public AddOilChangePage()
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