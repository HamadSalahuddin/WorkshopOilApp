using SQLite;
using SQLiteNetExtensions.Attributes;

namespace WorkshopOilApp.Models
{
    // Models/Customer.cs
    [Table("Customers")]
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int CustomerId { get; set; }

        [NotNull, Indexed]
        public string GivenName { get; set; } = string.Empty;

        [NotNull, Indexed]
        public string LastName { get; set; } = string.Empty;

        [NotNull]
        public string PhoneContact { get; set; } = string.Empty;

        public string? EmailAddress { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }

        [NotNull]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [NotNull]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        // Navigation
        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<Vehicle>? Vehicles { get; set; }

        public string FullName => $"{GivenName} {LastName}";
    }
}
