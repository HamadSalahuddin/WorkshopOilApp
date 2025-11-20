using SQLite;
using SQLiteNetExtensions.Attributes;

namespace WorkshopOilApp.Models
{

    [Table("Users")]
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }

        [NotNull, Indexed]
        public string GivenName { get; set; } = string.Empty;

        [NotNull]
        public string LastName { get; set; } = string.Empty;

        [NotNull, Unique, Indexed]
        public string UserName { get; set; } = string.Empty;

        [NotNull]
        public string HashedPassword { get; set; } = string.Empty;

        [NotNull]
        public string PassCode { get; set; } = string.Empty;

        [NotNull]
        public string BusinessName { get; set; } = string.Empty;

        [NotNull]
        public string BusinessContact { get; set; } = string.Empty;

        public string? BusinessEmail { get; set; }

        [NotNull]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        [NotNull]
        public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");

        public string FullName => $"{GivenName} {LastName}";
    }
}
