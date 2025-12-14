using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;
using WorkshopOilApp.Views;

namespace WorkshopOilApp.ViewModels
{
    // ViewModels/AddEditVehicleViewModel.cs
    public partial class AddEditVehicleViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty] string pageTitle = "Add Vehicle";
        [ObservableProperty] string saveButtonText = "Add Vehicle";

        [ObservableProperty] string registrationNumber = "";
        [ObservableProperty] string make = "";
        [ObservableProperty] string model = "";
        [ObservableProperty] string year = "";
        [ObservableProperty] string engine = "";
        [ObservableProperty] string oilCapacityLiters = "5.0";
        [ObservableProperty] string notes = "";

        [ObservableProperty] List<Lubricant> availableLubricants = new();
        [ObservableProperty] Lubricant? selectedLubricant;

        [ObservableProperty] string errorMessage = "";
        [ObservableProperty] bool hasError;
        [ObservableProperty] bool isBusy;

        private int CustomerId { get; set; }
        private int? VehicleId { get; set; }

        private readonly VehicleRepository _vehicles = new();
        private readonly LubricantRepository _lubricants = new();

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            CustomerId = Convert.ToInt32(query["customerId"]);

            if (query.TryGetValue("vehicleId", out var vid))
            {
                VehicleId = Convert.ToInt32(vid);
                PageTitle = "Edit Vehicle";
                SaveButtonText = "Save Changes";
            }

            // Initialize data without jumping to a background thread
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadLubricantsAsync();

            if (VehicleId.HasValue)
            {
                await LoadVehicleAsync();
            }
        }

        private async Task LoadLubricantsAsync()
        {
            var result = await _lubricants.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
            {
                AvailableLubricants = result.Data;
            }
        }

        private async Task LoadVehicleAsync()
        {
            IsBusy = true;
            var result = await _vehicles.GetWithChildrenAsync(VehicleId!.Value);
            if (!result.IsSuccess || result.Data == null)
            {
                ErrorMessage = result.ErrorMessage;
                HasError = true;
                IsBusy = false;
                return;
            }

            var vehicle = result.Data;

            RegistrationNumber = vehicle.RegistrationNumber;
            Make = vehicle.Make;
            Model = vehicle.Model;
            Year = vehicle.Year.ToString();
            Engine = vehicle.Engine ?? "";
            OilCapacityLiters = vehicle.OilCapacityLiters.ToString();
            Notes = vehicle.Notes ?? "";
            SelectedLubricant = AvailableLubricants.FirstOrDefault(l => l.LubricantId == vehicle.CurrentLubricantId);

            IsBusy = false;
        }

        [RelayCommand]
        async Task Save()
        {
            if (string.IsNullOrWhiteSpace(RegistrationNumber) || string.IsNullOrWhiteSpace(Make))
            {
                ErrorMessage = "Registration and Make are required";
                HasError = true;
                return;
            }

            IsBusy = true;

            Vehicle vehicle;
            if (VehicleId.HasValue)
            {
                var existingResult = await _vehicles.GetWithChildrenAsync(VehicleId.Value);
                if (!existingResult.IsSuccess || existingResult.Data == null)
                {
                    ErrorMessage = existingResult.ErrorMessage;
                    HasError = true;
                    IsBusy = false;
                    return;
                }

                vehicle = existingResult.Data;
            }
            else
            {
                vehicle = new Vehicle { CustomerId = CustomerId };
            }

            vehicle.RegistrationNumber = RegistrationNumber.Trim().ToUpper();
            vehicle.Make = Make.Trim();
            vehicle.Model = Model.Trim();
            vehicle.Year = int.TryParse(Year, out var y) ? y : DateTime.Today.Year;
            vehicle.Engine = string.IsNullOrWhiteSpace(Engine) ? null : Engine.Trim();
            vehicle.OilCapacityLiters = double.TryParse(OilCapacityLiters, out var cap) ? cap : 5.0;
            vehicle.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
            vehicle.CurrentLubricantId = SelectedLubricant?.LubricantId;

            if (VehicleId.HasValue)
            {
                var updateResult = await _vehicles.UpdateAsync(vehicle);
                if (!updateResult.IsSuccess)
                {
                    ErrorMessage = updateResult.ErrorMessage;
                    HasError = true;
                    IsBusy = false;
                    return;
                }
            }
            else
            {
                var insertResult = await _vehicles.InsertAsync(vehicle);
                if (!insertResult.IsSuccess)
                {
                    ErrorMessage = insertResult.ErrorMessage;
                    HasError = true;
                    IsBusy = false;
                    return;
                }
            }

            IsBusy = false;

            // Return to the customer detail page explicitly to avoid ambiguous route resolution
            var backRoute = $"//{nameof(CustomerListPage)}/{nameof(CustomerDetailPage)}?customerId={CustomerId}";
            await Shell.Current.GoToAsync(backRoute);
        }
    }
}
