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
    [ObservableProperty] string message = "";
    [ObservableProperty] Color messageColor = Colors.Green;
    [ObservableProperty] bool isBusy;
    [ObservableProperty] bool hasMessage;

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
        NextDueText = SelectedLubricant == null ? "Select oil first" : "Recommended interval applied";
        NextDueDateString = SelectedLubricant == null ? "" : ChangeDate.AddMonths(12).ToString("MMM dd, yyyy");
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

        IsBusy = false;
        UpdateDuePreview();
    }

    [RelayCommand]
    async Task Save()
    {
        if (SelectedLubricant == null)
        {
            Message = "Please select an oil type";
            MessageColor = Colors.Red;
            HasMessage = true;
            return;
        }

        IsBusy = true;
        HasMessage = false;

        var record = new OilChangeRecord
        {
            VehicleId = VehicleId,
            LubricantId = SelectedLubricant.LubricantId,
            ChangeDate = ChangeDate.ToUniversalTime().ToString("o"),
            MileageAtChange = mileage,
            Cost = cost > 0 ? cost : null,
            Notes = notes,
            NextRecommendedDate = ChangeDate.AddMonths(12).ToString("o"),  // or use vehicle interval
            CreatedAt = DateTime.UtcNow.ToString("o")
        };

        var db = await DatabaseService.InstanceAsync;
        await db.Db.InsertAsync(record);

        // Update vehicle's current lubricant
        Vehicle.CurrentLubricantId = SelectedLubricant.LubricantId;
        await db.Db.UpdateAsync(Vehicle);

        Message = "Oil change saved successfully!";
        MessageColor = Colors.Green;
        HasMessage = true;
        IsBusy = false;

        await Task.Delay(1500);
        await Shell.Current.GoToAsync("..");  // back to customer detail
    }
}