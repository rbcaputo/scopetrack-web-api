using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ScopeTrack.Domain.Entities;
using ScopeTrack.Domain.Enums;

namespace ScopeTrack.Infrastructure.Interceptors
{
  public sealed class ActivityLogInterceptor : SaveChangesInterceptor
  {
    public override InterceptionResult<int> SavingChanges(
      DbContextEventData eventData,
      InterceptionResult<int> result
    )
    {
      if (eventData.Context is not null)
        CreateActivityLogs(eventData.Context);

      return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
      DbContextEventData eventData,
      InterceptionResult<int> result,
      CancellationToken cancellationToken
    )
    {
      if (eventData.Context is not null)
        CreateActivityLogs(eventData.Context);

      return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void CreateActivityLogs(DbContext context)
    {
      List<EntityEntry> entries = [.. context.ChangeTracker.Entries()
        .Where(e => e.State is EntityState.Added or EntityState.Modified)
        .Where(e => e.Entity is ClientModel or ContractModel or DeliverableModel)];

      foreach (EntityEntry entry in entries)
      {
        ActivityLogModel? activityLog = entry switch
        {
          EntityEntry<ClientModel> client
            => CreateClientActivityLog(client),
          EntityEntry<ContractModel> contract
            => CreateContractActivityLog(contract, entry.State),
          EntityEntry<DeliverableModel> deliverable
            => CreateDeliverableActivityLog(deliverable, entry.State),
          _ => null
        };

        if (activityLog is not null)
          context.Add(activityLog);
      }
    }

    private static ActivityLogModel? CreateClientActivityLog(
      EntityEntry<ClientModel> entry
    )
    {
      switch (entry.State)
      {
        case EntityState.Added:
          return new(
            ActivityEntityType.Client,
            entry.Entity.Id,
            ActivityType.Created,
            $"Client '{entry.Entity.Name}' created"
          );
        case EntityState.Modified:
          List<PropertyEntry> modified
            = [.. entry.Properties.Where(p => p.IsModified)];
          if (modified.Count == 0)
            return null;

          if (modified.Any(p => p.Metadata.Name == nameof(ClientModel.Name)
                             || p.Metadata.Name == nameof(ClientModel.Email)
          ))
            return new(
              ActivityEntityType.Client,
              entry.Entity.Id,
              ActivityType.Updated,
              $"Client '{entry.Entity.Name}' updated"
            );
          else
            return new(
              ActivityEntityType.Client,
              entry.Entity.Id,
              ActivityType.StatusChanged,
              $"Client '{entry.Entity.Name}' status changed to {entry.Entity.Status}"
            );
        default:
          return null;
      }
    }

    private static ActivityLogModel? CreateContractActivityLog(
      EntityEntry<ContractModel> entry,
      EntityState state
    ) => state switch
    {
      EntityState.Added => new(
        ActivityEntityType.Contract,
        entry.Entity.Id,
        ActivityType.Created,
        $"Contract '{entry.Entity.Title}' created"
      ),
      EntityState.Modified => new(
        ActivityEntityType.Contract,
        entry.Entity.Id,
        ActivityType.StatusChanged,
        $"Contract '{entry.Entity.Title}' status changed to {entry.Entity.Status}"
      ),
      _ => null
    };

    private static ActivityLogModel? CreateDeliverableActivityLog(
      EntityEntry<DeliverableModel> entry,
      EntityState state
    ) => state switch
    {
      EntityState.Added => new(
        ActivityEntityType.Deliverable,
        entry.Entity.Id,
        ActivityType.Created,
        $"Deliverable '{entry.Entity.Title}' created"
      ),
      EntityState.Modified => new(
        ActivityEntityType.Deliverable,
        entry.Entity.Id,
        ActivityType.StatusChanged,
        $"Deliverable '{entry.Entity.Title}' status changed to {entry.Entity.Status}"
      ),
      _ => null
    };
  }
}
