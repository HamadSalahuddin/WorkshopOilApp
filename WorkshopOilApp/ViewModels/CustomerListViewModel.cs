// ViewModels/CustomerListViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLiteNetExtensionsAsync.Extensions;
using System.Collections.ObjectModel;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;
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
    private bool _hasMoreData = true;   // ← NEW: prevents extra loads

    public async void OnAppearing()
    {
        if (Customers.Count == 0 && !IsBusy)
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
        _hasMoreData = true;           // ← reset
        await LoadPage(append: false);  // ← false = replace
        IsBusy = false;
    }

    [RelayCommand]
    async Task LoadMore()
    {
        if (IsBusy || _isLoadingMore || !_hasMoreData) return;

        _isLoadingMore = true;
        _currentPage++;
        var addedCount = await LoadPage(append: true);

        // If we got fewer than PageSize → no more data
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
        var dbService = await DatabaseService.InstanceAsync;
        var db = dbService.Db;

        var query = db.Table<Customer>()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.GivenName);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var lower = SearchText.Trim().ToLower();
            query = query.Where(c =>
                c.GivenName.ToLower().Contains(lower) ||
                c.LastName.ToLower().Contains(lower));
        }

        var page = await query
            .Skip(_currentPage * PageSize)
            .Take(PageSize + 1)   // +1 to check if there's more
            .ToListAsync();

        if (page.Count == 0)
            return 0;

        var idsToLoad = page.Take(PageSize).Select(c => c.CustomerId).ToList();
        var fullCustomers = new List<Customer>();

        foreach (var id in idsToLoad)
        {
            var cust = await db.GetWithChildrenAsync<Customer>(id, recursive: true);
            if (cust != null) fullCustomers.Add(cust);
        }

        var cards = new List<CustomerCardViewModel>();
        foreach (var customer in fullCustomers)
        {
            var latestRecord = customer.Vehicles?
                .SelectMany(v => v.OilChangeRecords ?? new List<OilChangeRecord>())
                .OrderByDescending(r => r.ChangeDate)
                .FirstOrDefault();

            cards.Add(new CustomerCardViewModel(customer, latestRecord));
        }

        if (!append)
            Customers.Clear();

        foreach (var card in cards)
            Customers.Add(card);

        return cards.Count;  // return how many we actually added
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