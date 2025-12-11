using WorkshopOilApp.Helpers;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services.Repositories;

public class LubricantRepository : BaseRepository
{
    public async Task<Result<List<Lubricant>>> GetPageAsync(string? searchText, int skip, int takePlusOne)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var query = db.Table<Lubricant>().OrderBy(l => l.Name);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var lower = searchText.ToLower();
                query = query.Where(l =>
                    l.Name.ToLower().Contains(lower) ||
                    l.Viscosity.ToLower().Contains(lower));
            }

            var items = await query.Skip(skip).Take(takePlusOne).ToListAsync().ConfigureAwait(false);
            return Success(items);
        }
        catch (Exception ex)
        {
            return Failure<List<Lubricant>>($"Failed to load lubricants: {ex.Message}");
        }
    }

    public async Task<Result<List<Lubricant>>> GetAllAsync()
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var items = await db.Table<Lubricant>().ToListAsync().ConfigureAwait(false);
            return Success(items);
        }
        catch (Exception ex)
        {
            return Failure<List<Lubricant>>($"Failed to load lubricants: {ex.Message}");
        }
    }

    public async Task<Result<Lubricant>> GetByIdAsync(int id)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var l = await db.GetAsync<Lubricant>(id).ConfigureAwait(false);
            return Success(l);
        }
        catch (Exception ex)
        {
            return Failure<Lubricant>($"Failed to load lubricant: {ex.Message}");
        }
    }

    public async Task<Result<Lubricant>> InsertAsync(Lubricant lubricant)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.InsertAsync(lubricant).ConfigureAwait(false);
            return Success(lubricant);
        }
        catch (Exception ex)
        {
            return Failure<Lubricant>($"Failed to create lubricant: {ex.Message}");
        }
    }

    public async Task<Result<Lubricant>> UpdateAsync(Lubricant lubricant)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.UpdateAsync(lubricant).ConfigureAwait(false);
            return Success(lubricant);
        }
        catch (Exception ex)
        {
            return Failure<Lubricant>($"Failed to update lubricant: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int lubricantId)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.Table<Lubricant>()
                .DeleteAsync(l => l.LubricantId == lubricantId)
                .ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Failure($"Failed to delete lubricant: {ex.Message}");
        }
    }
}

