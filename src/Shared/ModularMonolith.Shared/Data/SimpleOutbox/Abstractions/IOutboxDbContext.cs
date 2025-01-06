using Microsoft.EntityFrameworkCore;
using ModularMonolith.Shared.Data.SimpleOutbox.Entities;

namespace ModularMonolith.Shared.Data.SimpleOutbox.Abstractions;

public interface IOutboxDbContext
{
  public DbSet<OutboxMessageEntity> OutboxMessages { get; }
}
