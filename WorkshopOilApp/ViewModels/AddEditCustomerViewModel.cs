// ViewModels/AddEditCustomerViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SQLiteNetExtensionsAsync.Extensions;
using System.Net.Mail;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;

namespace WorkshopOilApp.ViewModels;

public partial class AddEditCustomerViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] string pageTitle = "Add New Customer";
    [ObservableProperty] string saveButtonText = "Create Customer";

    [ObservableProperty] string givenName = "";
    [ObservableProperty] string lastName = "";
    [ObservableProperty] string phoneContact = "";
    [ObservableProperty] string emailAddress = "";
    [ObservableProperty] string address = "";
    [ObservableProperty] string notes = "";

    [ObservableProperty] string errorMessage = "";
    [ObservableProperty] bool hasError;
    [ObservableProperty] bool isBusy;
    [ObservableProperty] bool showSuccess;

    private int? CustomerId { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("customerId", out var idObj))
        {
            CustomerId = Convert.ToInt32(idObj);
            PageTitle = "Edit Customer";
            SaveButtonText = "Save Changes";
            _ = LoadCustomerAsync();
        }
    }

    private async Task LoadCustomerAsync()
    {
        IsBusy = true;
        var db = await DatabaseService.InstanceAsync;
        var customer = await db.Db.GetWithChildrenAsync<Customer>(CustomerId!.Value);

        GivenName = customer.GivenName;
        LastName = customer.LastName;
        PhoneContact = customer.PhoneContact;
        EmailAddress = customer.EmailAddress ?? "";
        Address = customer.Address ?? "";
        Notes = customer.Notes ?? "";

        IsBusy = false;
    }

    [RelayCommand]
    async Task Save()
    {
        HasError = false;
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(GivenName) ||
            string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(PhoneContact))
        {
            ErrorMessage = "Name and phone are required";
            HasError = true;
            return;
        }

        IsBusy = true;

        var customer = CustomerId.HasValue
            ? await (await DatabaseService.InstanceAsync).Db.GetWithChildrenAsync<Customer>(CustomerId.Value)
            : new Customer();

        customer.GivenName = GivenName.Trim();
        customer.LastName = LastName.Trim();
        customer.PhoneContact = PhoneContact.Trim();
        customer.EmailAddress = string.IsNullOrWhiteSpace(EmailAddress) ? null : EmailAddress.Trim();
        customer.Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim();
        customer.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
        customer.UpdatedAt = DateTime.UtcNow.ToString("o");

        if (!CustomerId.HasValue)
            customer.CreatedAt = DateTime.UtcNow.ToString("o");

        var db = await DatabaseService.InstanceAsync;

        if (CustomerId.HasValue)
            await db.Db.UpdateAsync(customer);
        else
            await db.Db.InsertAsync(customer);

        IsBusy = false;
        ShowSuccess = true;

        await Task.Delay(1200);
        await Shell.Current.GoToAsync("..");
    }
}