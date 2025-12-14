namespace WorkshopOilApp.Views;

public partial class OilChangeHistoryPage : ContentPage
{
    public OilChangeHistoryPage()
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