using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services;

public class DatabaseService
{
    private static readonly Lazy<Task<DatabaseService>> _instance
        = new(async () => await CreateInstanceAsync());

    public static Task<DatabaseService> InstanceAsync => _instance.Value;

    private SQLiteAsyncConnection? _db;

    public SQLiteAsyncConnection Db => _db!;

    private DatabaseService() { }  // private constructor

    private static async Task<DatabaseService> CreateInstanceAsync()
    {
        var service = new DatabaseService();
        await service.InitAsync();
        return service;
    }

    private async Task InitAsync()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "WorkshopDb.db3");

        var conn = new SQLiteAsyncConnection(
            databasePath, SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache
        );

        await conn.CreateTableAsync<User>();
        await conn.CreateTableAsync<Lubricant>();
        await conn.CreateTableAsync<Customer>();
        await conn.CreateTableAsync<Vehicle>();
        await conn.CreateTableAsync<OilChangeRecord>();

        // Seed common lubricants only on first launch
        var count = await conn.Table<Lubricant>().CountAsync();
        if (count == 0)
        {
            await conn.InsertAllAsync(new Lubricant[]
            {
                new() { Name = "Mobil 1 Extended Performance", Viscosity = "5W-30", Type = "FullSynthetic" },
                new() { Name = "Castrol EDGE", Viscosity = "0W-40", Type = "FullSynthetic" },
                new() { Name = "Pennzoil Platinum", Viscosity = "5W-20", Type = "FullSynthetic" },
                new() { Name = "Shell Rotella T6", Viscosity = "5W-40", Type = "FullSynthetic" },
            });
        }

        await conn.ExecuteAsync("CREATE INDEX IF NOT EXISTS IX_Customers_Name ON Customers(GivenName, LastName);");

        _db = conn;
    }

    //public async Task<T?> GetWithChildrenAsync<T>(int id, bool recursive = false) where T : class
    //{
    //    return await Db.GetWithChildrenAsync<T>(id, recursive);
    //}

    //public async Task<List<Customer>> GetCustomersWithDataAsync()
    //{
    //    var customers = await Db.Table<Customer>().ToListAsync();
    //    await Db.GetChildrenAsync(customers, recursive: true);
    //    return customers;
    //}
}