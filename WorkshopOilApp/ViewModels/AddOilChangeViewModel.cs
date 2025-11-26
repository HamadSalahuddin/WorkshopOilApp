// ViewModels/AddOilChangeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLiteNetExtensionsAsync.Extensions;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;

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
        var db = await DatabaseService.InstanceAsync;

        Customer = await db.Db.GetWithChildrenAsync<Customer>(CustomerId);
        Vehicle = Customer.Vehicles!.First(v => v.VehicleId == VehicleId);

        AvailableLubricants = await db.Db.Table<Lubricant>().ToListAsync();
        SelectedLubricant = AvailableLubricants.FirstOrDefault(l => l.LubricantId == Vehicle.CurrentLubricantId)
                           ?? AvailableLubricants.FirstOrDefault();
        NextRecommendedDate = ChangeDate.AddMonths(12);
        
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
        // Required fields
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

        // Business rule: Next date ≥ ChangeDate + 7 days
        if (NextRecommendedDate < ChangeDate.AddDays(7))
        {
            NextDateErrorVisible = true;
            SaveErrorMessage = "Next service date must be at least 7 days after change date";
            HasSaveError = true;
        }

        // Business rule: Next Km ≥ current + 500
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

        var db = await DatabaseService.InstanceAsync;
        await db.Db.InsertAsync(record);

        // Update vehicle's current lubricant
        Vehicle.CurrentLubricantId = SelectedLubricant.LubricantId;
        await db.Db.UpdateAsync(Vehicle);

        IsBusy = false;

        await Shell.Current.GoToAsync("..");  // back to customer detail
    }
}