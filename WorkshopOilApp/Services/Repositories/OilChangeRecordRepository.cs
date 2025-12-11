using SQLiteNetExtensionsAsync.Extensions;
using WorkshopOilApp.Helpers;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services.Repositories;

public class OilChangeRecordRepository : BaseRepository
{
    public async Task<Result<List<OilChangeRecord>>> GetHistoryPageAsync(
        int vehicleId,
        string startIso,
        string endIso,
        int skip,
        int takePlusOne)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var recordIds = await db.QueryScalarsAsync<int>(
                    "SELECT OilChangeId FROM OilChangeRecords WHERE VehicleId = ? AND ChangeDate >= ? AND ChangeDate <= ?",
                    vehicleId, startIso, endIso)
                .ConfigureAwait(false);

            var query = db.Table<OilChangeRecord>()
                .Where(r => recordIds.Contains(r.OilChangeId));

            var page = await query
                .OrderByDescending(r => r.ChangeDate)
                .Skip(skip)
                .Take(takePlusOne)
                .ToListAsync()
                .ConfigureAwait(false);

            if (page.Count == 0)
            {
                return Success(new List<OilChangeRecord>());
            }

            await db.GetChildrenAsync(page).ConfigureAwait(false);

            return Success(page);
        }
        catch (Exception ex)
        {
            return Failure<List<OilChangeRecord>>($"Failed to load oil change history: {ex.Message}");
        }
    }

    public async Task<Result<OilChangeRecord?>> GetForVehicleOnDateAsync(int vehicleId, string changeDateIso)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var record = await db.Table<OilChangeRecord>()
                .Where(r => r.VehicleId == vehicleId && r.ChangeDate == changeDateIso)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return Success(record);
        }
        catch (Exception ex)
        {
            return Failure<OilChangeRecord?>($"Failed to check existing record: {ex.Message}");
        }
    }

    public async Task<Result<OilChangeRecord>> InsertAsync(OilChangeRecord record)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.InsertAsync(record).ConfigureAwait(false);
            return Success(record);
        }
        catch (Exception ex)
        {
            return Failure<OilChangeRecord>($"Failed to create oil change record: {ex.Message}");
        }
    }

    public async Task<Result<bool>> HasRecordsForLubricantAsync(int lubricantId)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var any = (await db.Table<OilChangeRecord>()
                   .Where(r => r.LubricantId == lubricantId)
                   .ToListAsync())
                   .Any();

            return Success(any);
        }
        catch (Exception ex)
        {
            return Failure<bool>($"Failed to check oil change usage: {ex.Message}");
        }
    }
}
