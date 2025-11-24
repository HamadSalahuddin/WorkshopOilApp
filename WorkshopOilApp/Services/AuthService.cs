using WorkshopOilApp.Helpers;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services;

public class AuthService
{
    public async Task<Result<User>> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result<User>.Failure("Username is required");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Result<User>.Failure("Password is required");
        }

        var dbService = await DatabaseService.InstanceAsync;
        var user = await dbService.Db.Table<User>()
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());

        if (user == null)
        {
            return Result<User>.Failure("Invalid username or password");
        }

        if (!PasswordHasher.VerifyPassword(password, user.HashedPassword))
        {
            return Result<User>.Failure("Invalid username or password");
        }

        return Result<User>.Success(user);
    }

    public async Task<Result<User>> RegisterAsync(User user, string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(user.GivenName))
        {
            return Result<User>.Failure("Given Name is required");
        }

        if (string.IsNullOrWhiteSpace(user.LastName))
        {
            return Result<User>.Failure("Last Name is required");
        }

        if (string.IsNullOrWhiteSpace(user.BusinessName))
        {
            return Result<User>.Failure("Business Name is required");
        }

        if (string.IsNullOrWhiteSpace(user.UserName))
        {
            return Result<User>.Failure("Username is required");
        }

        if (string.IsNullOrWhiteSpace(user.PassCode))
        {
            return Result<User>.Failure("Passcode is required");
        }

        if (string.IsNullOrWhiteSpace(user.BusinessContact))
        {
            return Result<User>.Failure("Business Contact is required");
        }

        var dbService = await DatabaseService.InstanceAsync;
        var existing = await dbService.Db
            .Table<User>()
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == user.UserName.ToLower());

        if (existing != null)
        {
            return Result<User>.Failure("Username already exists");
        }

        user.HashedPassword = PasswordHasher.HashPassword(plainPassword);
        user.CreatedAt = DateTime.UtcNow.ToString("o");
        user.UpdatedAt = user.CreatedAt;

        await dbService.Db.InsertAsync(user);
        return Result<User>.Success(user);
    }

    public async Task<Result> ResetPasswordAsync(string username, string passcode, string newPassword)
    {
        var dbService = await DatabaseService.InstanceAsync;
        var user = await dbService.Db.Table<User>()
            .FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower() && u.PassCode == passcode);

        if (user == null)
        {
            return Result.Failure("Invalid username or passcode");
        }

        user.HashedPassword = PasswordHasher.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow.ToString("o");

        await dbService.Db.UpdateAsync(user);
        return Result.Success();
    }
}