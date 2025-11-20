using SQLite;

namespace WorkshopOilApp.Models
{
    // Models/Lubricant.cs
    [Table("Lubricants")]
    public class Lubricant
    {
        [PrimaryKey, AutoIncrement]
        public int LubricantId { get; set; }

        [NotNull]
        public string Name { get; set; } = string.Empty;           // e.g. Mobil 1 5W-30

        [NotNull]
        public string Viscosity { get; set; } = string.Empty;      // 5W-30

        public string? ApiSpec { get; set; }                       // SP, CK-4 etc.

        [NotNull]
        public string Type { get; set; } = "FullSynthetic";        // Mineral, SemiSynthetic, FullSynthetic, HighMileage

        public string? Notes { get; set; }

        [NotNull]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        public string DisplayName => $"{Name} {Viscosity}";
    }
}
