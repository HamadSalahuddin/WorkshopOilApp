// ViewModels/CustomerCardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.ViewModels;

public partial class CustomerCardViewModel : ObservableObject
{
    [ObservableProperty] string fullName;
    [ObservableProperty] string phoneContact;
    [ObservableProperty] string vehicleSummary;[ObservableProperty] ObservableCollection<VehicleEngineOilStatus> vehicleStatuses = new();

    public Customer Customer { get; }

    public CustomerCardViewModel(Customer customer, List<OilChangeRecord> latestRecords)
    {
        Customer = customer;
        FullName = customer.FullName;
        PhoneContact = customer.PhoneContact ?? "No phone";

        // Vehicle summary
        var count = customer.Vehicles?.Count ?? 0;
        VehicleSummary = count == 0 ? "No vehicles" :
                        count == 1 ? "1 vehicle" : $"{count} vehicles";

        if (latestRecords == null ||
            latestRecords.Count == 0
        )
        {
            var vechileEngineOilStatus = new VehicleEngineOilStatus();
            vechileEngineOilStatus.StatusText = "No Oil Change";
            vechileEngineOilStatus.StatusColor = Colors.Gray;
            vechileEngineOilStatus.DaysText = "-";
            vechileEngineOilStatus.DaysTextColor = Colors.Gray;
            return;
        }

        foreach (var latestRecord in latestRecords)
        {
            var daysUntilDue = (latestRecord.NextRecommendedDateLocal - DateTime.Today).Value.Days;
            var vehicleEngineOilStatus = new VehicleEngineOilStatus();
            vehicleEngineOilStatus.Registration = latestRecord.Vehicle.RegistrationNumber;
            if (daysUntilDue < 0)
            {
                vehicleEngineOilStatus.StatusText = "OVERDUE";
                vehicleEngineOilStatus.StatusColor = Colors.Red;
                vehicleEngineOilStatus.DaysText = $"{Math.Abs(daysUntilDue)} days ago";
                vehicleEngineOilStatus.DaysTextColor = Colors.Red;
            }
            else if (daysUntilDue <= 7)
            {
                vehicleEngineOilStatus.StatusText = "DUE SOON";
                vehicleEngineOilStatus.StatusColor = Colors.Orange;
                vehicleEngineOilStatus.DaysText = $"{daysUntilDue} days left";
                vehicleEngineOilStatus.DaysTextColor = Colors.Orange;
            }
            else
            {
                vehicleEngineOilStatus.StatusText = "ON TRACK";
                vehicleEngineOilStatus.StatusColor = Colors.Green;
                vehicleEngineOilStatus.DaysText = $"{daysUntilDue} days left";
                vehicleEngineOilStatus.DaysTextColor = Colors.Green;
            }

            vehicleStatuses.Add(vehicleEngineOilStatus);
        }
    }
}

public class VehicleEngineOilStatus
{
    public string Registration { get; set; }
    public string StatusText { get; set; }
    public Color StatusColor { get; set; }
    public string DaysText { get; set; }
    public Color DaysTextColor { get; set; }
}