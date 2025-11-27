// ViewModels/OilChangeHistoryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.ObjectModel;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;

namespace WorkshopOilApp.ViewModels;

public partial class OilChangeHistoryViewModel : ObservableObject, IQueryAttributable
{
    private const int PageSize = 20;

    [ObservableProperty] Vehicle vehicle = new();
    [ObservableProperty]
    private ObservableCollection<OilChangeCardViewModel> history = new();

    [ObservableProperty] DateTime startDate = DateTime.Today.AddYears(-2);
    [ObservableProperty] DateTime endDate = DateTime.Today;
    [ObservableProperty] bool isBusy;
    [ObservableProperty] bool isRefreshing;

    public DateTime Today => DateTime.Today;

    private int VehicleId { get; set; }

    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMore = true;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("vehicleId", out var id))
        {
            VehicleId = Convert.ToInt32(id);
            _ = LoadHistoryAsync();
        }
    }
    public async void OnAppearing()
    {
        if (History.Count == 0 && !IsBusy)
        {
            await LoadHistoryAsync();
        }
    }

    [RelayCommand]
    async Task ApplyFilter()
    {
        IsBusy = true;
        _currentPage = 0;
        History.Clear();
        _hasMore = true;
        await LoadPageAsync();
        IsBusy = false;
    }

    [RelayCommand]
    async Task LoadMore()
    {
        if (IsBusy || _isLoadingMore || !_hasMore) return;
        _isLoadingMore = true;
        _currentPage++;
        await LoadPageAsync();
        _isLoadingMore = false;
    }

    [RelayCommand]
    async Task Refresh()
    {
        IsRefreshing = true;
        _currentPage = 0;
        History.Clear();
        _hasMore = true;
        await LoadPageAsync();
        IsRefreshing = false;
    }

    private async Task LoadPageAsync()
    {
        var db = await DatabaseService.InstanceAsync;

        var startIso = ToIsoString(StartDate);
        var endIso = ToIsoString(EndDate.AddDays(1));
        var recordIds = await db.Db.QueryScalarsAsync<int>(
            "SELECT OilChangeId FROM OilChangeRecords WHERE VehicleId = ? AND ChangeDate >= ? AND ChangeDate <= ?",
            VehicleId, startIso, endIso
        );

        var query = db.Db.Table<OilChangeRecord>()
        .Where(r => recordIds.Contains(r.OilChangeId));

        List<OilChangeRecord> page;
        try
        {
            page = await query
            .OrderByDescending(r => r.ChangeDate)
            .Skip(_currentPage * PageSize)
            .Take(PageSize + 1)
            .ToListAsync();
        }
        catch(Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Exception :{e.Message}");
            throw;
        }

        if (page?.Count == 0)
        {
            _hasMore = false;
            return;
        }

        await db.Db.GetChildrenAsync(page);

        var oilChangeRecords = page.Take(PageSize).ToList();

        foreach (var oilChangeRecord in oilChangeRecords)
        {
            oilChangeRecord.Lubricant = await db.Db.GetAsync<Lubricant>(lubricant => lubricant.LubricantId == oilChangeRecord.LubricantId);
            History.Add(new OilChangeCardViewModel(oilChangeRecord));
        }

        _hasMore = page.Count > PageSize;
    }

    private string ToIsoString(DateTime date) => date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

    // First load
    private async Task LoadHistoryAsync()
    {
        IsBusy = true;
        await LoadPageAsync();
        IsBusy = false;
    }
}