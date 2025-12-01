using WorkshopOilApp.ViewModels;

namespace WorkshopOilApp.Views;

public partial class LubricantListPage : ContentPage
{
    private DateTime _lastSearchTime;
    private const int SearchDelayMs = 600;

    public LubricantListPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LubricantListViewModel vm)
        {
            vm.OnAppearing();
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var now = DateTime.Now;
        _lastSearchTime = now;

        await Task.Delay(SearchDelayMs);

        // Only execute if no new typing happened
        if ((DateTime.Now - _lastSearchTime).TotalMilliseconds >= SearchDelayMs - 10)
        {
            if (BindingContext is LubricantListViewModel vm && !vm.IsBusy)
            {
                await vm.LoadLubricantsCommand.ExecuteAsync(null);
            }
        }
    }
}