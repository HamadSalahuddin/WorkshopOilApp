using WorkshopOilApp.ViewModels;

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
            Command = new Command(async () =>
            {
                if (BindingContext is AddEditVehicleViewModel vm)
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