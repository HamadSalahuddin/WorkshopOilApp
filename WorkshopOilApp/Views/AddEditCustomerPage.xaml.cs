using WorkshopOilApp.ViewModels;

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
            Command = new Command(async () =>
            {
                if (BindingContext is AddEditCustomerViewModel vm)
                {
                    var backRoute = vm.CustomerId.HasValue
                        ? $"//{nameof(CustomerListPage)}/{nameof(CustomerDetailPage)}?customerId={vm.CustomerId}"
                        : $"//{nameof(CustomerListPage)}";

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