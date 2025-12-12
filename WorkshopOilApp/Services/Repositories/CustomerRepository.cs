using SQLiteNetExtensionsAsync.Extensions;
using WorkshopOilApp.Helpers;
using WorkshopOilApp.Models;

namespace WorkshopOilApp.Services.Repositories;

public class CustomerRepository : BaseRepository
{
    public async Task<Result<Customer>> GetWithVehiclesAsync(int customerId)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var customer = await db.GetWithChildrenAsync<Customer>(customerId, recursive: true)
                .ConfigureAwait(false);

            if (customer == null)
            {
                return Failure<Customer>("Customer not found");
            }

            return Success(customer);
        }
        catch (Exception ex)
        {
            return Failure<Customer>($"Failed to load customer: {ex.Message}");
        }
    }

    public async Task<Result<List<Customer>>> GetPageAsync(string? searchText, int skip, int take)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            var query = db.Table<Customer>()
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.GivenName);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var lower = searchText.Trim().ToLower();
                query = query.Where(c =>
                    c.GivenName.ToLower().Contains(lower) ||
                    c.LastName.ToLower().Contains(lower));
            }

            var items = await query.Skip(skip).Take(take).ToListAsync().ConfigureAwait(false);
            return Success(items);
        }
        catch (Exception ex)
        {
            return Failure<List<Customer>>($"Failed to load customers: {ex.Message}");
        }
    }

    public async Task<Result<Customer>> InsertAsync(Customer customer)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.InsertAsync(customer).ConfigureAwait(false);
            return Success(customer);
        }
        catch (Exception ex)
        {
            return Failure<Customer>($"Failed to create customer: {ex.Message}");
        }
    }

    public async Task<Result<Customer>> UpdateAsync(Customer customer)
    {
        try
        {
            var db = await GetDbAsync().ConfigureAwait(false);
            await db.UpdateAsync(customer).ConfigureAwait(false);
            return Success(customer);
        }
        catch (Exception ex)
        {
            return Failure<Customer>($"Failed to update customer: {ex.Message}");
        }
    }
}

