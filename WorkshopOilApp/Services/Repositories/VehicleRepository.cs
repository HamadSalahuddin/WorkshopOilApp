using SQLiteNetExtensionsAsync.Extensions;
using WorkshopOilApp.Helpers;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services.Repositories;

public class VehicleRepository : BaseRepository
{
    public async Task<Result<Vehicle>> GetWithChildrenAsync(int vehicleId)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var vehicle = await db.GetWithChildrenAsync<Vehicle>(vehicleId).ConfigureAwait(false);
            if (vehicle == null)
                return Failure<Vehicle>("Vehicle not found");

            return Success(vehicle);
        }
        catch (Exception ex)
        {
            return Failure<Vehicle>($"Failed to load vehicle: {ex.Message}");
        }
    }

    public async Task<Result<Vehicle>> InsertAsync(Vehicle vehicle)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.InsertAsync(vehicle).ConfigureAwait(false);
            return Success(vehicle);
        }
        catch (Exception ex)
        {
            return Failure<Vehicle>($"Failed to create vehicle: {ex.Message}");
        }
    }

    public async Task<Result<Vehicle>> UpdateAsync(Vehicle vehicle)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.UpdateAsync(vehicle).ConfigureAwait(false);
            return Success(vehicle);
        }
        catch (Exception ex)
        {
            return Failure<Vehicle>($"Failed to update vehicle: {ex.Message}");
        }
    }
}

