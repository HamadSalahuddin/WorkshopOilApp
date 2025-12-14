using WorkshopOilApp.Views;

namespace WorkshopOilApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ForgotPasswordPage), typeof(ForgotPasswordPage));
            Routing.RegisterRoute(nameof(CustomerListPage), typeof(CustomerListPage));
            Routing.RegisterRoute(nameof(CustomerDetailPage), typeof(CustomerDetailPage));
            Routing.RegisterRoute(nameof(AddOilChangePage), typeof(AddOilChangePage));
            Routing.RegisterRoute(nameof(OilChangeHistoryPage), typeof(OilChangeHistoryPage));
            Routing.RegisterRoute(nameof(AddEditCustomerPage), typeof(AddEditCustomerPage));
            Routing.RegisterRoute(nameof(AddEditVehiclePage), typeof(AddEditVehiclePage));
            Routing.RegisterRoute(nameof(LubricantListPage), typeof(LubricantListPage));
            Routing.RegisterRoute(nameof(AddEditLubricantPage), typeof(AddEditLubricantPage));
            Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
            //    Routing.RegisterRoute(nameof(VehicleDetailPage.PageRoute, typeof(VehicleDetailPage));
            //Routing.RegisterRoute(nameof(OilChangeHistoryPage), typeof(OilChangeHistoryPage));
            //    Routing.RegisterRoute(nameof(AddEditCustomerPage), typeof(AddEditCustomerPage));
            //    Routing.RegisterRoute(nameof(AddEditVehiclePage), typeof(AddEditVehiclePage));
            //    Routing.RegisterRoute(nameof(AddOilChangePage), typeof(AddOilChangePage));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}