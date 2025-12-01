// ViewModels/LubricantListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;
using WorkshopOilApp.Views;

namespace WorkshopOilApp.ViewModels;

public partial class LubricantListViewModel : ObservableObject
{
    private const int PageSize = 30;

    [ObservableProperty] ObservableCollection<LubricantCardViewModel> lubricants = new();
    [ObservableProperty] string searchText = "";
    [ObservableProperty] bool isRefreshing;
    [ObservableProperty] bool isBusy;

    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMore = true;

    public async void OnAppearing()
    {
        if (!IsBusy)
        {
            await LoadLubricants();
        }
    }

    [RelayCommand]
    async Task LoadLubricants()
    {
        if (IsBusy) return;

        IsBusy = true;
        Lubricants.Clear();
        _currentPage = 0;
        _hasMore = true;
        await LoadPage();
        IsBusy = false;
    }

    [RelayCommand]
    async Task LoadMore()
    {
        if (IsBusy || _isLoadingMore || !_hasMore) return;

        _isLoadingMore = true;
        _currentPage++;
        await LoadPage();
        _isLoadingMore = false;
    }

    [RelayCommand]
    async Task Refresh()
    {
        IsRefreshing = true;
        _currentPage = 0;
        Lubricants.Clear();
        await LoadLubricants();
        IsRefreshing = false;
    }

    private async Task LoadPage()
    {
        var db = await DatabaseService.InstanceAsync;

        var query = db.Db.Table<Lubricant>().OrderBy(l => l.Name);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var lower = SearchText.ToLower();
            query = query.Where(l =>
                l.Name.ToLower().Contains(lower) ||
                l.Viscosity.ToLower().Contains(lower));
        }

        var items = await query
            .Skip(_currentPage * PageSize)
            .Take(PageSize + 1)
            .ToListAsync();

        foreach (var l in items.Take(PageSize))
            Lubricants.Add(new LubricantCardViewModel(l));

        _hasMore = items.Count > PageSize;
    }

    [RelayCommand]
    async Task AddLubricant() => await Shell.Current.GoToAsync(nameof(AddEditLubricantPage));

    [RelayCommand]
    async Task EditLubricant(LubricantCardViewModel card)
        => await Shell.Current.GoToAsync($"{nameof(AddEditLubricantPage)}?id={card.Lubricant.LubricantId}");

    [RelayCommand]
    async Task DeleteLubricant(LubricantCardViewModel card)
    {
        var confirmDelete = await Shell.Current.DisplayAlert(
            "Confirmation",
            $"Are you sure that you want to delete {card.Name.ToUpper()} - {card.Viscosity}",
            "OK",
            "Cancel"
        );

        if (!confirmDelete)
        {
            return;
        }
        var db = await DatabaseService.InstanceAsync;

        var lubricantOildChanges = await db.Db.Table<OilChangeRecord>()
            .Where(oilChangeRecord => oilChangeRecord.LubricantId == card.Lubricant.LubricantId)
            .ToListAsync();

        if (lubricantOildChanges.Any())
        {
            await Shell.Current.DisplayAlert("Item Used", "Item you are trying to delete has been used for oil change"," OK");
            return;
        }

        await db.Db.Table<Lubricant>()
            .DeleteAsync(l => l.LubricantId == card.Lubricant.LubricantId);


        await LoadLubricants();
    }
}