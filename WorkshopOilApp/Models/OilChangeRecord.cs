using SQLite;
using SQLiteNetExtensions.Attributes;

namespace WorkshopOilApp.Models
{
    // Models/OilChangeRecord.cs
    [Table("OilChangeRecords")]
    public class OilChangeRecord
    {
        [PrimaryKey, AutoIncrement]
        public int OilChangeId { get; set; }

        [ForeignKey(typeof(Vehicle))]
        public int VehicleId { get; set; }

        [ForeignKey(typeof(Lubricant))]
        public int LubricantId { get; set; }

        [NotNull]
        public string ChangeDate { get; set; } = DateTime.UtcNow.ToString("o");

        [NotNull]
        public double MileageAtChange { get; set; }

        public string? NextRecommendedDate { get; set; }   // ISO8601
        public double? NextRecommendedKm { get; set; }
        public double? Cost { get; set; }
        public string? Notes { get; set; }
        public string? LastReminderSentDate { get; set; }  // null = not sent

        [NotNull]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        // Relationships
        [ManyToOne]
        public Vehicle? Vehicle { get; set; }

        [ManyToOne]
        public Lubricant? Lubricant { get; set; }

        public DateTime ChangeDateLocal => DateTime.Parse(ChangeDate).ToLocalTime();
        public DateTime? NextRecommendedDateLocal =>
            NextRecommendedDate != null ? DateTime.Parse(NextRecommendedDate).ToLocalTime() : null;
    }
}
