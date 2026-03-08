using CaseItau.Infra.Data;
using CaseItau.Infra.Interfaces;

namespace CaseItau.Infra.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            // In a real application, you might want to get change tracking information here to log or handle specific cases before saving.
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task RollbackAsync()
        {
            // Entity Framework Core doesn't support explicit rollback outside of transactions.
            // For more advanced scenarios, use transactions:
            // await _context.Database.RollbackTransactionAsync();
            return Task.CompletedTask;
        }
    }
}
