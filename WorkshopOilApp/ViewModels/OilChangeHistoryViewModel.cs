// ViewModels/OilChangeHistoryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;

namespace WorkshopOilApp.ViewModels;

public partial class OilChangeHistoryViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] Vehicle vehicle = new();
    [ObservableProperty] ObservableCollection<OilChangeRecord> history = new();
    [ObservableProperty] string searchText = "";
    [ObservableProperty] bool isRefreshing;

    private int VehicleId { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("vehicleId", out var id))
        {
            VehicleId = Convert.ToInt32(id);
            _ = LoadHistory();
        }
    }

    [RelayCommand]
    async Task LoadHistory()
    {
        IsRefreshing = true;
        var db = await DatabaseService.InstanceAsync;

        var records = await db.Db.Table<OilChangeRecord>()
            .Where(r => r.VehicleId == VehicleId)
            .OrderByDescending(r => r.ChangeDate)
            .ToListAsync();

        History.Clear();
        foreach (var r in records)
        {
            r.Lubricant = await db.Db.GetAsync<Lubricant>(lubricant => lubricant.LubricantId == r.LubricantId);

            History.Add(r);
        }

        IsRefreshing = false;
    }

    [RelayCommand] async Task Refresh() => await LoadHistory();
}