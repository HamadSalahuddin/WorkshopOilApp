using WorkshopOilApp.Helpers;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services.Repositories;

public class UserRepository : BaseRepository
{
    public async Task<Result<User?>> GetByUserNameAsync(string userName)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var lower = userName.ToLower();
            var user = await db.Table<User>()
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == lower)
                .ConfigureAwait(false);

            return Result<User?>.Success(user);
        }
        catch (Exception ex)
        {
            return Failure<User?>($"Failed to load user: {ex.Message}");
        }
    }

    public async Task<Result<User?>> GetByUserNameAndPasscodeAsync(string userName, string passcode)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var lower = userName.ToLower();
            var user = await db.Table<User>()
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == lower && u.PassCode == passcode)
                .ConfigureAwait(false);

            return Success(user);
        }
        catch (Exception ex)
        {
            return Failure<User?>($"Failed to load user: {ex.Message}");
        }
    }

    public async Task<Result<User>> InsertAsync(User user)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.InsertAsync(user).ConfigureAwait(false);
            return Success(user);
        }
        catch (Exception ex)
        {
            return Failure<User>($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(User user)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.UpdateAsync(user).ConfigureAwait(false);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Failure($"Failed to update user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UserNameExistsAsync(string userName)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var lower = userName.ToLower();
            var existing = await db.Table<User>()
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == lower)
                .ConfigureAwait(false);

            return Success(existing != null);
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Failed to check username: {ex.Message}");
        }
    }
}
