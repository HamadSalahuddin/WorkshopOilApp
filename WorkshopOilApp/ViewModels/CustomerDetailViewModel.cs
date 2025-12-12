// ViewModels/CustomerDetailViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;
using WorkshopOilApp.Views;

namespace WorkshopOilApp.ViewModels;

public partial class CustomerDetailViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] Customer customer = new();
    [ObservableProperty] ObservableCollection<VehicleCardViewModel> vehicles = new();
    [ObservableProperty] string vehicleCountText = "";
    [ObservableProperty] bool isRefreshing;
    [ObservableProperty] bool isBusy;

    private int CustomerId { get; set; }

    private readonly CustomerRepository _customers = new();
    private readonly LubricantRepository _lubricants = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("customerId", out var id))
            CustomerId = Convert.ToInt32(id);

        OnAppearing();
    }

    public async void OnAppearing()
    {
        if (!IsBusy)
        {
            await LoadCustomerWithVehiclesAsync();
        }
    }

    [RelayCommand]
    public async Task LoadCustomerWithVehiclesAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        Vehicles.Clear();
        await LoadCustomerAsync();
        IsBusy = false;
    }

    [RelayCommand]
    async Task LoadCustomerAsync()
    {
        var result = await _customers.GetWithVehiclesAsync(CustomerId);
        if (!result.IsSuccess || result.Data == null)
        {
            // keep it simple: just stop if load failed
            return;
        }

        Customer = result.Data;

        foreach (var vehicle in Customer.Vehicles ?? new())
        {
            var latest = vehicle.OilChangeRecords?
                .OrderByDescending(r => r.ChangeDate)
                .FirstOrDefault();

            if (vehicle.CurrentLubricantId != null)
            {
                var lubeResult = await _lubricants.GetByIdAsync(vehicle.CurrentLubricantId.Value);
                if (lubeResult.IsSuccess)
                {
                    vehicle.CurrentLubricant = lubeResult.Data;
                }
            }

            Vehicles.Add(new VehicleCardViewModel(vehicle, latest));
        }

        VehicleCountText = Customer.Vehicles?.Count == 1
            ? "1 vehicle registered"
            : $"{Customer.Vehicles?.Count ?? 0} vehicles registered";
    }

    [RelayCommand]
    async Task Refresh()
    {
        IsRefreshing = true;
        Vehicles.Clear();
        await LoadCustomerWithVehiclesAsync();
        IsRefreshing = false;
    }

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

