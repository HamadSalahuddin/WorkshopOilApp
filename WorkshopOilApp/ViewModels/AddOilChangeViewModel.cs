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

    public string CustomerName => customer.FullName;
    public string NextDueText => SelectedLubricant == null ? "Select oil first" : "Recommended interval applied";
    public string NextDueDateString => SelectedLubricant == null ? "" :
        ChangeDate.AddMonths(12).AddDays(-7).ToString("MMM dd, yyyy");

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("customerId", out var cid))
            _ = LoadDataAsync(Convert.ToInt32(cid), query);
    }

    private async Task LoadDataAsync(int customerId, IDictionary<string, object> query)
    {
        IsBusy = true;
        var db = await DatabaseService.InstanceAsync;

        Customer = await db.Db.GetWithChildrenAsync<Customer>(customerId);
        if (query.TryGetValue("vehicleId", out var vid))
        {
            var vId = Convert.ToInt32(vid);
            Vehicle = Customer.Vehicles!.First(v => v.VehicleId == vId);
            VehicleId = vId;
        }

        AvailableLubricants = await db.Db.Table<Lubricant>().ToListAsync();
        SelectedLubricant = AvailableLubricants.FirstOrDefault(l => l.LubricantId == Vehicle.CurrentLubricantId)
                           ?? AvailableLubricants.FirstOrDefault();

        IsBusy = false;
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