using WorkshopOilApp.Helpers;
using WorkshopOilApp.Models;
using WorkshopOilApp.Services.Repositories;

namespace WorkshopOilApp.Services;

public class AuthService
{
    private readonly UserRepository _users = new();

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

        var userResult = await _users.GetByUserNameAsync(username);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Result<User>.Failure("Invalid username or password");
        }

        if (!PasswordHasher.VerifyPassword(password, userResult.Data.HashedPassword))
        {
            return Result<User>.Failure("Invalid username or password");
        }

        return Result<User>.Success(userResult.Data);
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

        var existsResult = await _users.UserNameExistsAsync(user.UserName);
        if (!existsResult.IsSuccess)
        {
            return Result<User>.Failure(existsResult.ErrorMessage);
        }

        if (existsResult.Data)
        {
            return Result<User>.Failure("Username already exists");
        }

        user.HashedPassword = PasswordHasher.HashPassword(plainPassword);
        user.CreatedAt = DateTime.UtcNow.ToString("o");
        user.UpdatedAt = user.CreatedAt;

        var insertResult = await _users.InsertAsync(user);
        if (!insertResult.IsSuccess)
        {
            return Result<User>.Failure(insertResult.ErrorMessage);
        }

        return insertResult;
    }

    public async Task<Result> ResetPasswordAsync(string username, string passcode, string newPassword)
    {
        var userResult = await _users.GetByUserNameAndPasscodeAsync(username, passcode);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return Result.Failure("Invalid username or passcode");
        }

        userResult.Data.HashedPassword = PasswordHasher.HashPassword(newPassword);
        userResult.Data.UpdatedAt = DateTime.UtcNow.ToString("o");

        var updateResult = await _users.UpdateAsync(userResult.Data);
        if (!updateResult.IsSuccess)
        {
            return Result.Failure(updateResult.ErrorMessage);
        }

        return Result.Success();
    }
}

