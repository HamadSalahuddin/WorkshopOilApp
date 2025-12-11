// ViewModels/CustomerListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;
using WorkshopOilApp.Views;

namespace WorkshopOilApp.ViewModels;

public partial class CustomerListViewModel : ObservableObject
{
    private const int PageSize = 5;

    [ObservableProperty] ObservableCollection<CustomerCardViewModel> customers = new();
    [ObservableProperty] string searchText = "";
    [ObservableProperty] bool isRefreshing;
    [ObservableProperty] bool isBusy;

    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMoreData = true;

    private readonly CustomerRepository _customersRepo = new();

    public async void OnAppearing()
    {
        if (!IsBusy)
        {
            await LoadCustomers();
        }
    }

    [RelayCommand]
    async Task LoadCustomers()
    {
        if (IsBusy) return;

        IsBusy = true;
        Customers.Clear();
        _currentPage = 0;
        _hasMoreData = true;
        await LoadPage(append: false);
        IsBusy = false;
    }

    [RelayCommand]
    async Task LoadMore()
    {
        if (IsBusy || _isLoadingMore || !_hasMoreData) return;

        _isLoadingMore = true;
        _currentPage++;
        var addedCount = await LoadPage(append: true);

        if (addedCount < PageSize)
            _hasMoreData = false;

        _isLoadingMore = false;
    }

    [RelayCommand]
    async Task Refresh()
    {
        IsRefreshing = true;
        await LoadCustomers();
        IsRefreshing = false;
    }

    private async Task<int> LoadPage(bool append)
    {
        var pageResult = await _customersRepo.GetPageAsync(
            SearchText,
            _currentPage * PageSize,
            PageSize + 1);

        if (!pageResult.IsSuccess || pageResult.Data == null || pageResult.Data.Count == 0)
            return 0;

        var idsToLoad = pageResult.Data.Take(PageSize).Select(c => c.CustomerId).ToList();
        var fullCustomers = new List<Customer>();

        foreach (var id in idsToLoad)
        {
            var custResult = await _customersRepo.GetWithVehiclesAsync(id);
            if (custResult.IsSuccess && custResult.Data != null)
            {
                fullCustomers.Add(custResult.Data);
            }
        }

        var cards = new List<CustomerCardViewModel>();
        foreach (var customer in fullCustomers)
        {
            var latestRecord = customer.Vehicles?
                .SelectMany(v => v.OilChangeRecords ?? new List<OilChangeRecord>())
                .OrderByDescending(r => r.ChangeDate)
                .GroupBy(oil => oil.VehicleId)
                .Select(kvp => kvp.First())
                .ToList();

            cards.Add(new CustomerCardViewModel(customer, latestRecord));
        }

        if (!append)
            Customers.Clear();

        foreach (var card in cards)
            Customers.Add(card);

        return cards.Count;
    }

    [RelayCommand]
    async Task AddCustomer()
    {
        Customers.Clear();
        await Shell.Current.GoToAsync(nameof(AddEditCustomerPage));
    }

    [RelayCommand]
    async Task SelectCustomer(CustomerCardViewModel card)
        => await Shell.Current.GoToAsync($"{nameof(CustomerDetailPage)}?customerId={card.Customer.CustomerId}");
}

