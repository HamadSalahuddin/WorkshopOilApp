using SQLite;
using WorkshopOilApp.Helpers;
using WorkshopOilApp.Services;

namespace WorkshopOilApp.Services.Repositories;

public abstract class BaseRepository
{
    protected async Task<SQLiteAsyncConnection> GetDbAsync()
        => (await DatabaseService.InstanceAsync).Db;

    protected static Result<T> Success<T>(T data) => Result<T>.Success(data);

    protected static Result Failure(string error) => Result.Failure(error);

    protected static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
}
