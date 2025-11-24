// ViewModels/VehicleCardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.ViewModels;

public partial class VehicleCardViewModel : ObservableObject
{
    public Vehicle Vehicle { get; }
    public OilChangeRecord? LatestRecord { get; }

    [ObservableProperty] string fullModel;
    [ObservableProperty] string registrationNumber;
    [ObservableProperty] string currentOil;
    [ObservableProperty] string oilCapacity;
    [ObservableProperty] string statusText;
    [ObservableProperty] Color statusColor;
    [ObservableProperty] string daysText;
    [ObservableProperty] Color daysTextColor;

    public VehicleCardViewModel(Vehicle vehicle, OilChangeRecord? latestRecord)
    {
        Vehicle = vehicle;
        LatestRecord = latestRecord;

        FullModel = vehicle.FullModel;
        RegistrationNumber = vehicle.RegistrationNumber;
        OilCapacity = $"{vehicle.OilCapacityLiters}L";
        CurrentOil = vehicle.CurrentLubricant?.DisplayName ?? "Not set";

        if (latestRecord == null || latestRecord.NextRecommendedDateLocal == null)
        {
            StatusText = "NO RECORD";
            StatusColor = Colors.Gray;
            DaysText = "-";
            DaysTextColor = Colors.Gray;
            return;
        }

        var days = (latestRecord.NextRecommendedDateLocal.Value.Date - DateTime.Today).Days;

        if (days < 0)
        {
            StatusText = "OVERDUE";
            StatusColor = Colors.Red;
            DaysText = $"{Math.Abs(days)}d ago";
            DaysTextColor = Colors.Red;
        }
        else if (days <= 7)
        {
            StatusText = "DUE SOON";
            StatusColor = Colors.Orange;
            DaysText = $"{days}d left";
            DaysTextColor = Colors.Orange;
        }
        else
        {
            StatusText = "GOOD";
            StatusColor = Colors.Green;
            DaysText = $"{days}d left";
            DaysTextColor = Colors.Green;
        }
    }
}