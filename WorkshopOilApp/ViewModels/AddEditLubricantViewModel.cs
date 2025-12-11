// ViewModels/AddEditLubricantViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;

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

    private readonly LubricantRepository _lubricants = new();

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
        if (!LubricantId.HasValue)
            return;

        IsBusy = true;
        var result = await _lubricants.GetByIdAsync(LubricantId.Value);
        if (!result.IsSuccess || result.Data == null)
        {
            ErrorMessage = result.ErrorMessage;
            HasError = true;
            IsBusy = false;
            return;
        }

        var l = result.Data;

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
        HasError = false;
        ErrorMessage = "";

        Lubricant l;
        if (LubricantId.HasValue)
        {
            var existing = await _lubricants.GetByIdAsync(LubricantId.Value);
            if (!existing.IsSuccess || existing.Data == null)
            {
                ErrorMessage = existing.ErrorMessage;
                HasError = true;
                IsBusy = false;
                return;
            }

            l = existing.Data;
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
        {
            var updateResult = await _lubricants.UpdateAsync(l);
            if (!updateResult.IsSuccess)
            {
                ErrorMessage = updateResult.ErrorMessage;
                HasError = true;
                IsBusy = false;
                return;
            }
        }
        else
        {
            var insertResult = await _lubricants.InsertAsync(l);
            if (!insertResult.IsSuccess)
            {
                ErrorMessage = insertResult.ErrorMessage;
                HasError = true;
                IsBusy = false;
                return;
            }
        }

        IsBusy = false;
        await Shell.Current.GoToAsync("..");
    }
}

