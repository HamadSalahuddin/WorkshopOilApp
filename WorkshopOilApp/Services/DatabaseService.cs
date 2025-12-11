using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services;

public class DatabaseService
{
    private static DatabaseService? _instance;
    private static readonly System.Threading.SemaphoreSlim _initLock = new(1, 1);

    public static DatabaseService Instance =>
        _instance ?? throw new InvalidOperationException("DatabaseService is not initialized. Call InitializeAsync() first.");

    public static Task<DatabaseService> InstanceAsync => GetOrCreateInstanceAsync();

    public static Task InitializeAsync() => GetOrCreateInstanceAsync();

    public SQLiteAsyncConnection Db { get; }

    private DatabaseService(SQLiteAsyncConnection db)
    {
        Db = db;
    }

    private static async Task<DatabaseService> GetOrCreateInstanceAsync()
    {
        if (_instance != null)
            return _instance;

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_instance != null)
                return _instance;

            var conn = await CreateAndInitializeConnectionAsync().ConfigureAwait(false);
            _instance = new DatabaseService(conn);
            return _instance;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static async Task<SQLiteAsyncConnection> CreateAndInitializeConnectionAsync()
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

        return conn;
    }
}
