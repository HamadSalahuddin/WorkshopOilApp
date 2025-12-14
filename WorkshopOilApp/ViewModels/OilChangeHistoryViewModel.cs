// ViewModels/OilChangeHistoryViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;

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

    public int VehicleId { get; private set; }
    public int? CustomerId { get; private set; }

    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMore = true;

    private readonly OilChangeRecordRepository _oilChanges = new();
    private readonly LubricantRepository _lubricants = new();
    private readonly VehicleRepository _vehicles = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("vehicleId", out var id))
        {
            VehicleId = Convert.ToInt32(id);
            _ = LoadHistoryAsync();
        }

        if (query.TryGetValue("customerId", out var customerId))
        {
            CustomerId = Convert.ToInt32(customerId);
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
        var startIso = ToIsoString(StartDate);
        var endIso = ToIsoString(EndDate.AddDays(1));

        var pageResult = await _oilChanges.GetHistoryPageAsync(
            VehicleId,
            startIso,
            endIso,
            _currentPage * PageSize,
            PageSize + 1);

        if (!pageResult.IsSuccess || pageResult.Data == null || pageResult.Data.Count == 0)
        {
            _hasMore = false;
            return;
        }

        var page = pageResult.Data;

        var oilChangeRecords = page.Take(PageSize).ToList();

        foreach (var oilChangeRecord in oilChangeRecords)
        {
            if (oilChangeRecord.Lubricant == null)
            {
                var lubeResult = await _lubricants.GetByIdAsync(oilChangeRecord.LubricantId);
                if (lubeResult.IsSuccess)
                {
                    oilChangeRecord.Lubricant = lubeResult.Data;
                }
            }

            History.Add(new OilChangeCardViewModel(oilChangeRecord));
        }

        _hasMore = page.Count > PageSize;
    }

    private string ToIsoString(DateTime date) => date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

    // First load
    private async Task LoadHistoryAsync()
    {
        IsBusy = true;

        if (CustomerId is null)
        {
            var vehicleResult = await _vehicles.GetByIdAsync(VehicleId);
            if (vehicleResult.IsSuccess && vehicleResult.Data != null)
            {
                Vehicle = vehicleResult.Data;
                CustomerId = vehicleResult.Data.CustomerId;
            }
        }

        await LoadPageAsync();
        IsBusy = false;
    }
}

