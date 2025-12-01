// ViewModels/LubricantCardViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.ViewModels;

public partial class LubricantCardViewModel : ObservableObject
{
    public Lubricant Lubricant { get; }

    public string Name => Lubricant.Name;
    public string Viscosity => Lubricant.Viscosity;
    public string Type => Lubricant.Type;
    public string ApiSpec => Lubricant.ApiSpec ?? "";
    public bool HasApiSpec => !string.IsNullOrWhiteSpace(ApiSpec);

    public LubricantCardViewModel(Lubricant lubricant) => Lubricant = lubricant;
}