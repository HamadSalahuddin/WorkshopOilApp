// Services/DatabaseService.cs
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services;

public class DatabaseService
{
    private static DatabaseService? _instance;
    public static DatabaseService Instance => _instance ??= new DatabaseService();

    private SQLiteAsyncConnection? _db;

    private DatabaseService() { }

    public SQLiteAsyncConnection Db => _db ??= Init().GetAwaiter().GetResult();

    // Services/DatabaseService.cs
    private async Task<SQLiteAsyncConnection> Init()
    {
        try
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "WorkshopDb.db3");

            var conn = new SQLiteAsyncConnection(databasePath);

            // This automatically creates the database file + all tables if they don't exist
            await conn.CreateTableAsync<User>();
            await conn.CreateTableAsync<Lubricant>();
            await conn.CreateTableAsync<Customer>();
            await conn.CreateTableAsync<Vehicle>();
            await conn.CreateTableAsync<OilChangeRecord>();

            // Optional: Seed some common lubricants on first launch only
            var lubricantCount = await conn.Table<Lubricant>().CountAsync();
            if (lubricantCount == 0)
            {
                await conn.InsertAllAsync(new[]
                {
                    new Lubricant { Name = "Mobil 1 Extended Performance", Viscosity = "5W-30", Type = "FullSynthetic" },
                    new Lubricant { Name = "Castrol EDGE", Viscosity = "5W-30", Type = "FullSynthetic" },
                    new Lubricant { Name = "Valvoline SynPower", Viscosity = "5W-20", Type = "FullSynthetic" },
                    new Lubricant { Name = "Shell Rotella T6", Viscosity = "5W-40", Type = "FullSynthetic" },
                    // Add more common oils here if you want
                });
            }

            return conn;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DB Init Error: {ex}");
            throw;
        }

    }

    //// Helper: Get with children (recursive)
    //public async Task<T?> GetWithChildrenAsync<T>(int id, bool recursive = false) where T : class
    //{
    //    return await Db.GetWithChildrenAsync<T>(id, recursive);
    //}

    // Example: Get all customers with vehicles and latest oil change
    public async Task<List<Customer>> GetCustomersWithDataAsync()
    {
        var customers = await Db.Table<Customer>().ToListAsync();
        await Db.GetChildrenAsync(customers, recursive: true);
        return customers;
    }
}