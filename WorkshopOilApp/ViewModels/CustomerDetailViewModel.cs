// ViewModels/CustomerDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.ObjectModel;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;
using WorkshopOilApp.Views;

namespace WorkshopOilApp.ViewModels;

public partial class CustomerDetailViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] Customer customer = new();
    [ObservableProperty] ObservableCollection<VehicleCardViewModel> vehicles = new();
    [ObservableProperty] string vehicleCountText = "";
    [ObservableProperty] bool isRefreshing;

    private int CustomerId { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("customerId", out var id))
            CustomerId = Convert.ToInt32(id);

        OnAppearing();
    }

    public async void OnAppearing()
    {
        await LoadCustomerAsync();
    }

    [RelayCommand]
    async Task LoadCustomerAsync()
    {
        IsRefreshing = true;
        var db = await DatabaseService.InstanceAsync;

        Customer = await db.Db.GetWithChildrenAsync<Customer>(CustomerId, recursive: true);

        if (Customer == null) return;

        Vehicles.Clear();
        foreach (var vehicle in Customer.Vehicles ?? new())
        {
            var latest = vehicle.OilChangeRecords?
                .OrderByDescending(r => r.ChangeDate)
                .FirstOrDefault();

            var currentLubricant = vehicle.CurrentLubricantId != null 
                ? await db.Db.GetAsync<Lubricant>(l => l.LubricantId == vehicle.CurrentLubricantId)
                : null;

            vehicle.CurrentLubricant = currentLubricant;

            Vehicles.Add(new VehicleCardViewModel(vehicle, latest));
        }

        VehicleCountText = Customer.Vehicles?.Count == 1
            ? "1 vehicle registered"
            : $"{Customer.Vehicles?.Count ?? 0} vehicles registered";

        IsRefreshing = false;
    }

    [RelayCommand]
    async Task Refresh() => await LoadCustomerAsync();

    [RelayCommand]
    async Task ViewOilHistory(VehicleCardViewModel card)
    {
        await Shell.Current.GoToAsync($"{nameof(OilChangeHistoryPage)}?vehicleId={card.Vehicle.VehicleId}");
    }

    [RelayCommand]
    async Task AddVehicle()
    => await Shell.Current.GoToAsync($"{nameof(AddEditVehiclePage)}?customerId={Customer.CustomerId}");

    [RelayCommand]
    async Task EditVehicle(VehicleCardViewModel card)
        => await Shell.Current.GoToAsync($"{nameof(AddEditVehiclePage)}?customerId={Customer.CustomerId}&vehicleId={card.Vehicle.VehicleId}");

    [RelayCommand]
    async Task EditCustomer()
        => await Shell.Current.GoToAsync($"{nameof(AddEditCustomerPage)}?customerId={Customer.CustomerId}");

    [RelayCommand]
    async Task AddOilChangeForVehicle(VehicleCardViewModel card)
    {
        var route = $"{nameof(AddOilChangePage)}?customerId={Customer.CustomerId}&vehicleId={card.Vehicle.VehicleId}";
        await Shell.Current.GoToAsync(route);
    }
}