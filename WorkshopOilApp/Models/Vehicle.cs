using SQLite;
using SQLiteNetExtensions.Attributes;

namespace WorkshopOilApp.Models
{
    // Models/Vehicle.cs
    [Table("Vehicles")]
    public class Vehicle
    {
        [PrimaryKey, AutoIncrement]
        public int VehicleId { get; set; }

        [ForeignKey(typeof(Customer))]
        public int CustomerId { get; set; }

        [NotNull, Indexed]
        public string RegistrationNumber { get; set; } = string.Empty;

        [NotNull]
        public string Make { get; set; } = string.Empty;

        [NotNull]
        public string Model { get; set; } = string.Empty;

        [NotNull]
        public int Year { get; set; }

        public string? Engine { get; set; }
        public double OilCapacityLiters { get; set; } = 5.0;

        public int? CurrentLubricantId { get; set; }

        public string? Notes { get; set; }

        [NotNull]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [NotNull]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        // Relationships
        [ManyToOne]
        public Customer? Customer { get; set; }

        [ManyToOne]
        [ForeignKey(typeof(Lubricant))]
        public Lubricant? CurrentLubricant { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<OilChangeRecord>? OilChangeRecords { get; set; }

        public string FullModel => $"{Year} {Make} {Model}";
    }
}
