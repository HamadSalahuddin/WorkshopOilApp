// ViewModels/AddEditLubricantViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services;

namespace WorkshopOilApp.ViewModels;


public partial class AddEditLubricantViewModel : ObservableObject, IQueryAttributable
{
    [ObservableProperty] string pageTitle = "Add Lubricant";
    [ObservableProperty] string saveButtonText = "Add Lubricant";

    [ObservableProperty] string name = "";
    [ObservableProperty] string viscosity = "";
    [ObservableProperty] string apiSpec = "";
    [ObservableProperty] string notes = "";
    [ObservableProperty] string selectedType = "FullSynthetic";

    [ObservableProperty] string errorMessage = "";
    [ObservableProperty] bool hasError;
    [ObservableProperty] bool isBusy;

    public List<string> OilTypes => new() { "FullSynthetic", "SemiSynthetic", "Mineral", "HighMileage" };

    private int? LubricantId { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id))
        {
            LubricantId = Convert.ToInt32(id);
            PageTitle = "Edit Lubricant";
            SaveButtonText = "Save Changes";
            _ = LoadLubricant();
        }
    }

    private async Task LoadLubricant()
    {
        IsBusy = true;
        var db = await DatabaseService.InstanceAsync;
        var l = await db.Db.GetAsync<Lubricant>(LubricantId!.Value);

        Name = l.Name;
        Viscosity = l.Viscosity;
        ApiSpec = l.ApiSpec ?? "";
        SelectedType = l.Type;
        Notes = l.Notes ?? "";
        IsBusy = false;
    }

    [RelayCommand]
    async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Viscosity))
        {
            ErrorMessage = "Name and Viscosity are required";
            HasError = true;
            return;
        }

        IsBusy = true;
        var db = await DatabaseService.InstanceAsync;

        Lubricant l;
        if (LubricantId.HasValue)
        {
            l = await db.Db.GetAsync<Lubricant>(LubricantId.Value);
        }
        else
        {
            l = new Lubricant();
        }

        l.Name = Name.Trim();
        l.Viscosity = Viscosity.Trim();
        l.ApiSpec = string.IsNullOrWhiteSpace(ApiSpec) ? null : ApiSpec.Trim();
        l.Type = SelectedType;
        l.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();

        if (LubricantId.HasValue)
            await db.Db.UpdateAsync(l);
        else
            await db.Db.InsertAsync(l);

        IsBusy = false;
        await Shell.Current.GoToAsync("..");
    }
}