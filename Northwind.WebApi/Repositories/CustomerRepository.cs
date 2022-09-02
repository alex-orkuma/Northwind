using Microsoft.Data.SqlClient.Server;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Packt.Shared;
using System.Collections.Concurrent;

namespace Northwind.WebApi.Repositories;

public class CustomerRepository : ICustomerRepository
{
    // use a static thread-safe dictionary field to cache the customers
    public static ConcurrentDictionary<string, Customer> customersCache;

    // use an instance data context field because it should not be
    // cached due to their internal caching

    private NorthwindContext db;
    public CustomerRepository(NorthwindContext injectedContex)
    {
        db = injectedContex;

          // pre - load customers from database as a normal
         // Dictionary with CustomerId as the key,
         // then convert to a thread-safe ConcurrentDictionary

        if(customersCache is null)
        {
            customersCache = new ConcurrentDictionary<string, Customer>(db.Customers.ToDictionary(c => c.CustomerId));
        }

    }

    public async Task<Customer?> CreateAsync(Customer c)
    {
        // Change custome id into uppper case
        c.CustomerId = c.CustomerId.ToUpper();

        EntityEntry<Customer> added = await db.Customers.AddAsync(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            if (customersCache is null) return c;
            return customersCache.AddOrUpdate(c.CustomerId, c, UpdateCache);
        }
        else
        {
            return null;
        }
        
    }

    // for performance get from cache
    public Task<IEnumerable<Customer>> RetrieveAllAsync()
    {
        return Task.FromResult(customersCache is null ? Enumerable.Empty<Customer>() : customersCache.Values);
    }

    public Task<Customer> RetrieveAsync(string id)
    {
        // for perfomance, get from cache
        id = id.ToUpper();
        if (customersCache is null) return null!;
        customersCache.TryGetValue(id, out Customer? c);

        return Task.FromResult(c);

    }

    private Customer UpdateCache(string id, Customer c)
    {
        Customer? old;
        if (customersCache is not null)
        {
            if (customersCache.TryGetValue(id, out old))
            {
                if(customersCache.TryUpdate(id, c, old))
                {
                    return c;
                }
            }
        }

        return null!;
    }

    public async Task<Customer?> UpdateAsync(string id, Customer c)
    {
        id = id.ToUpper();
        c.CustomerId = c.CustomerId.ToUpper();

        db.Customers.Update(c);
        int affected = await db.SaveChangesAsync();

        if (affected == 1)
        {
            // update in cache
            return UpdateCache(id, c);
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(string id)
    {
        id = id.ToUpper();

        Customer? c = db.Customers.Find(id);
        if (c is null) return null;
        db.Customers.Remove(c);
        int affected = await db.SaveChangesAsync();

        if (affected == 1)
        {
            if (customersCache is null) return null;
            // remove from cache
            return customersCache.TryRemove(id, out c);

        }
        else
        {
            return null;
        }
    }
}