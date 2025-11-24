// ViewModels/CustomerCardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.ViewModels;

public partial class CustomerCardViewModel : ObservableObject
{
    [ObservableProperty] string fullName;
    [ObservableProperty] string phoneContact;
    [ObservableProperty] string vehicleSummary;
    [ObservableProperty] string statusText;
    [ObservableProperty] Color statusColor;
    [ObservableProperty] string daysText;
    [ObservableProperty] Color daysTextColor;

    public Customer Customer { get; }

    public CustomerCardViewModel(Customer customer, OilChangeRecord? latestRecord)
    {
        Customer = customer;
        FullName = customer.FullName;
        PhoneContact = customer.PhoneContact ?? "No phone";

        // Vehicle summary
        var count = customer.Vehicles?.Count ?? 0;
        VehicleSummary = count == 0 ? "No vehicles" :
                        count == 1 ? "1 vehicle" : $"{count} vehicles";

        if (latestRecord == null)
        {
            StatusText = "No Oil Change";
            StatusColor = Colors.Gray;
            DaysText = "-";
            DaysTextColor = Colors.Gray;
            return;
        }

        var daysUntilDue = (latestRecord.NextRecommendedDateLocal - DateTime.Today).Value.Days;

        if (daysUntilDue < 0)
        {
            StatusText = "OVERDUE";
            StatusColor = Colors.Red;
            DaysText = $"{Math.Abs(daysUntilDue)} days ago";
            DaysTextColor = Colors.Red;
        }
        else if (daysUntilDue <= 7)
        {
            StatusText = "DUE SOON";
            StatusColor = Colors.Orange;
            DaysText = $"{daysUntilDue} days left";
            DaysTextColor = Colors.Orange;
        }
        else
        {
            StatusText = "ON TRACK";
            StatusColor = Colors.Green;
            DaysText = $"{daysUntilDue} days left";
            DaysTextColor = Colors.Green;
        }
    }
}