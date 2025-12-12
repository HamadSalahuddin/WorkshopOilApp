// ViewModels/LubricantListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;
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

    private readonly LubricantRepository _lubricantsRepo = new();
    private readonly OilChangeRecordRepository _oilChangesRepo = new();

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
        var itemsResult = await _lubricantsRepo.GetPageAsync(
            SearchText,
            _currentPage * PageSize,
            PageSize + 1);

        if (!itemsResult.IsSuccess || itemsResult.Data == null)
        {
            _hasMore = false;
            return;
        }

        var items = itemsResult.Data;

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

        var usedResult = await _oilChangesRepo.HasRecordsForLubricantAsync(card.Lubricant.LubricantId);
        if (!usedResult.IsSuccess)
        {
            await Shell.Current.DisplayAlert("Error", usedResult.ErrorMessage, "OK");
            return;
        }

        if (usedResult.Data)
        {
            await Shell.Current.DisplayAlert("Item Used", "Item you are trying to delete has been used for oil change", "OK");
            return;
        }

        var deleteResult = await _lubricantsRepo.DeleteAsync(card.Lubricant.LubricantId);
        if (!deleteResult.IsSuccess)
        {
            await Shell.Current.DisplayAlert("Error", deleteResult.ErrorMessage, "OK");
            return;
        }

        await LoadLubricants();
    }
}

