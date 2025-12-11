// ViewModels/AddOilChangeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;

namespace WorkshopOilApp.ViewModels;

public partial class AddOilChangeViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] Customer customer = new();
    [ObservableProperty] Vehicle vehicle = new();
    [ObservableProperty] List<Lubricant> availableLubricants = new();
    [ObservableProperty] Lubricant? selectedLubricant;
    [ObservableProperty] DateTime changeDate = DateTime.Today;
    [ObservableProperty] double mileage;
    [ObservableProperty] double cost;
    [ObservableProperty] string notes = "";
    [ObservableProperty] bool isBusy;
    [ObservableProperty] private DateTime nextRecommendedDate;
    [ObservableProperty] private string nextRecommendedKm = "";
    [ObservableProperty] private string saveErrorMessage = "";
    [ObservableProperty] private bool hasSaveError;

    [ObservableProperty] private bool nextDateErrorVisible;
    [ObservableProperty] private bool nextKmErrorVisible;

    public DateTime MinimumNextDate => ChangeDate.AddDays(7);

    private int VehicleId { get; set; }
    private int CustomerId { get; set; }

    public string CustomerName => customer.FullName;

    [ObservableProperty]
    private string _nextDueText = "Select oil first";

    [ObservableProperty]
    private string _nextDueDateString = "";

    private readonly CustomerRepository _customers = new();
    private readonly LubricantRepository _lubricants = new();
    private readonly VehicleRepository _vehicles = new();
    private readonly OilChangeRecordRepository _oilChanges = new();

    partial void OnSelectedLubricantChanged(Lubricant? oldValue, Lubricant? newValue)
        => UpdateDuePreview();

    partial void OnChangeDateChanged(DateTime oldValue, DateTime newValue)
        => UpdateDuePreview();

    private void UpdateDuePreview()
    {
        NextRecommendedDate = ChangeDate.AddMonths(12);  // default suggestion
        OnPropertyChanged(nameof(MinimumNextDate));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        CustomerId = Convert.ToInt32(query["customerId"]);

        if (query.TryGetValue("vehicleId", out var vid))
        {
            VehicleId = Convert.ToInt32(vid);
            _ = LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        IsBusy = true;

        var customerResult = await _customers.GetWithVehiclesAsync(CustomerId);
        if (!customerResult.IsSuccess || customerResult.Data == null)
        {
            SaveErrorMessage = customerResult.ErrorMessage;
            HasSaveError = true;
            IsBusy = false;
            return;
        }

        Customer = customerResult.Data;
        Vehicle = Customer.Vehicles!.First(v => v.VehicleId == VehicleId);

        var lubesResult = await _lubricants.GetAllAsync();
        if (lubesResult.IsSuccess && lubesResult.Data != null)
        {
            AvailableLubricants = lubesResult.Data;
        }

        SelectedLubricant = AvailableLubricants.FirstOrDefault(l => l.LubricantId == Vehicle.CurrentLubricantId)
                           ?? AvailableLubricants.FirstOrDefault();
        NextRecommendedDate = ChangeDate.AddMonths(3);

        IsBusy = false;
        UpdateDuePreview();
    }

    [RelayCommand]
    async Task Save()
    {
        SaveErrorMessage = "";
        HasSaveError = false;
        NextDateErrorVisible = false;
        NextKmErrorVisible = false;

        if (SelectedLubricant == null)
        {
            SaveErrorMessage = "Please select an oil type";
            HasSaveError = true;
            return;
        }

        if (Mileage <= 0)
        {
            SaveErrorMessage = "Please enter current mileage";
            HasSaveError = true;
            return;
        }

        if (NextRecommendedDate < ChangeDate.AddDays(7))
        {
            NextDateErrorVisible = true;
            SaveErrorMessage = "Next service date must be at least 7 days after change date";
            HasSaveError = true;
        }

        if (!double.TryParse(NextRecommendedKm, out var nextKm) || nextKm < Mileage + 500)
        {
            NextKmErrorVisible = true;
            SaveErrorMessage = "Next recommended km must be at least 500 km more than current";
            HasSaveError = true;
        }

        if (HasSaveError) return;

        IsBusy = true;

        var record = new OilChangeRecord
        {
            VehicleId = Vehicle.VehicleId,
            LubricantId = SelectedLubricant.LubricantId,
            ChangeDate = ChangeDate.ToUniversalTime().ToString("o"),
            MileageAtChange = Mileage,
            Cost = !string.IsNullOrWhiteSpace(Cost.ToString()) ? Cost : null,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
            NextRecommendedDate = NextRecommendedDate.ToUniversalTime().ToString("o"),
            NextRecommendedKm = nextKm,
            CreatedAt = DateTime.UtcNow.ToString("o")
        };

        var existingResult = await _oilChanges.GetForVehicleOnDateAsync(record.VehicleId, record.ChangeDate);
        if (!existingResult.IsSuccess)
        {
            SaveErrorMessage = existingResult.ErrorMessage;
            HasSaveError = true;
            IsBusy = false;
            return;
        }

        if (existingResult.Data != null)
        {
            SaveErrorMessage = $"Record for {Vehicle.FullModel} on {ChangeDate.Date:d} already exists.";
            HasSaveError = true;
            IsBusy = false;
            return;
        }

        var insertRecordResult = await _oilChanges.InsertAsync(record);
        if (!insertRecordResult.IsSuccess)
        {
            SaveErrorMessage = insertRecordResult.ErrorMessage;
            HasSaveError = true;
            IsBusy = false;
            return;
        }

        Vehicle.CurrentLubricantId = SelectedLubricant.LubricantId;
        var updateVehicleResult = await _vehicles.UpdateAsync(Vehicle);
        if (!updateVehicleResult.IsSuccess)
        {
            SaveErrorMessage = updateVehicleResult.ErrorMessage;
            HasSaveError = true;
            IsBusy = false;
            return;
        }

        IsBusy = false;

        await Shell.Current.GoToAsync("..");
    }
}

