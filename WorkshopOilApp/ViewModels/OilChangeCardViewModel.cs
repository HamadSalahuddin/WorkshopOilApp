// ViewModels/OilChangeCardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.ViewModels;

public partial class OilChangeCardViewModel : ObservableObject
{
    public OilChangeRecord Record { get; }

    [ObservableProperty] private string lubricantName = "";
    [ObservableProperty] private string dateDisplay = "";
    [ObservableProperty] private string mileageDisplay = "";
    [ObservableProperty] private string costDisplay = "";
    [ObservableProperty] private string nextDueDisplay = "";
    [ObservableProperty] private bool hasCost;
    [ObservableProperty] private bool hasNotes;
    [ObservableProperty] private string notesToDisplay = "";

    public OilChangeCardViewModel(OilChangeRecord record)
    {
        Record = record;

        LubricantName = record.Lubricant?.DisplayName ?? "Unknown Oil";
        DateDisplay = record.ChangeDateLocal.ToString("dd MMM yyyy · dddd");
        MileageDisplay = $"{record.MileageAtChange:N0} km";

        if (record.Cost.HasValue)
        {
            CostDisplay = $"${record.Cost.Value:F2}";
            HasCost = true;
        }

        if (!string.IsNullOrWhiteSpace(record.Notes))
        {

            HasNotes = true;
            NotesToDisplay = record.Notes;
        }
            

        if (record.NextRecommendedDateLocal.HasValue)
        {
            var daysLeft = (record.NextRecommendedDateLocal.Value.Date - DateTime.Today).Days;
            var color = daysLeft < 0 ? Colors.Red : daysLeft <= 7 ? Colors.Orange : Colors.Green;
            NextDueDisplay = daysLeft < 0
                ? $"OVERDUE by {Math.Abs(daysLeft)} days"
                : $"Due in {daysLeft} days";
            NextDueColor = color;
        }
        else
        {
            NextDueDisplay = "Not set";
            NextDueColor = Colors.Gray;
        }
    }

    [ObservableProperty] private Color nextDueColor = Colors.Gray;
}