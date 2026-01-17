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
      EntityEntry<ClientModel> client
    )
    {
      switch (client.State)
      {
        case EntityState.Added:
          return new(
            ActivityEntityType.Client,
            client.Entity.ID,
            ActivityType.Created,
            $"Client '{client.Entity.Name}' created"
          );
        case EntityState.Modified:
          List<PropertyEntry> modified
            = [.. client.Properties.Where(p => p.IsModified)];
          if (modified.Count == 0)
            return null;

          if (modified.Any(p => p.Metadata.Name == nameof(ClientModel.Name)
                             || p.Metadata.Name == nameof(ClientModel.Email)
          ))
            return new(
              ActivityEntityType.Client,
              client.Entity.ID,
              ActivityType.Updated,
              $"Client '{client.Entity.Name}' updated"
            );
          else
            return new(
              ActivityEntityType.Client,
              client.Entity.ID,
              ActivityType.StatusChanged,
              $"Client '{client.Entity.ID}' status changed to {client.Entity.Status}"
            );
        default:
          return null;
      }
    }

    private static ActivityLogModel? CreateContractActivityLog(
      EntityEntry<ContractModel> contract,
      EntityState state
    ) => state switch
    {
      EntityState.Added => new(
        ActivityEntityType.Contract,
        contract.Entity.ID,
        ActivityType.Created,
        $"Contract '{contract.Entity.Title}' created"
      ),
      EntityState.Modified => new(
        ActivityEntityType.Contract,
        contract.Entity.ID,
        ActivityType.StatusChanged,
        $"Contract '{contract.Entity.Title}' status changed to {contract.Entity.Status}"
      ),
      _ => null
    };

    private static ActivityLogModel? CreateDeliverableActivityLog(
      EntityEntry<DeliverableModel> deliverable,
      EntityState state
    ) => state switch
    {
      EntityState.Added => new(
        ActivityEntityType.Deliverable,
        deliverable.Entity.ID,
        ActivityType.Created,
        $"Deliverable '{deliverable.Entity.Title}' created"
      ),
      EntityState.Modified => new(
        ActivityEntityType.Deliverable,
        deliverable.Entity.ID,
        ActivityType.StatusChanged,
        $"Deliverable '{deliverable.Entity.ID}' status changed to {deliverable.Entity.Status}"
      ),
      _ => null
    };
  }
}
