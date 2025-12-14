using WorkshopOilApp.ViewModels;

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
            Command = new Command(async () =>
            {
                if (BindingContext is OilChangeHistoryViewModel vm && vm.CustomerId.HasValue)
                {
                    var backRoute = $"//{nameof(CustomerListPage)}/{nameof(CustomerDetailPage)}?customerId={vm.CustomerId}";
                    await Shell.Current.GoToAsync(backRoute);
                }
                else
                {
                    await Shell.Current.GoToAsync("..");
                }
            })
        });
    }
}