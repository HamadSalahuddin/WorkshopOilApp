using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLiteNetExtensionsAsync.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;

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

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            CustomerId = Convert.ToInt32(query["customerId"]);
            if (query.TryGetValue("vehicleId", out var vid))
            {
                VehicleId = Convert.ToInt32(vid);
                PageTitle = "Edit Vehicle";
                SaveButtonText = "Save Changes";
                _ = LoadVehicleAsync();
            }
            _ = LoadLubricantsAsync();
        }

        private async Task LoadLubricantsAsync()
        {
            var db = await DatabaseService.InstanceAsync;
            AvailableLubricants = await db.Db.Table<Lubricant>().ToListAsync();
        }

        private async Task LoadVehicleAsync()
        {
            IsBusy = true;
            var db = await DatabaseService.InstanceAsync;
            var vehicle = await db.Db.GetWithChildrenAsync<Vehicle>(VehicleId!.Value);

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
            var db = await DatabaseService.InstanceAsync;

            Vehicle vehicle;
            if (VehicleId.HasValue)
            {
                vehicle = await db.Db.GetWithChildrenAsync<Vehicle>(VehicleId.Value);
            }
            else
            {
                vehicle = new Vehicle { CustomerId = CustomerId };
                await db.Db.InsertAsync(vehicle);
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
                await db.Db.UpdateAsync(vehicle);
            else
                await db.Db.InsertAsync(vehicle);

            IsBusy = false;
            await Shell.Current.GoToAsync("..");
        }
    }
}
