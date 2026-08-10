using BookingService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookingService.Infrastructure.Interceptors;

public class UpdateAuditableEntitiesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var dbContext = eventData.Context;

        if (dbContext == null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        
        var entries = dbContext.ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(a => a.CreatedAt).CurrentValue = DateTime.UtcNow;
            }
            
            if (entry.State == EntityState.Modified)
            {
                entry.Property(a => a.ModifiedAt).CurrentValue = DateTime.UtcNow;
            }
        }
        
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}