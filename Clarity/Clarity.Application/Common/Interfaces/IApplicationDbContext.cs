using Clarity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clarity.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<TaskItem> TaskItems { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
