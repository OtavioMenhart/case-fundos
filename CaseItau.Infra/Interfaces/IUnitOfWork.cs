namespace CaseItau.Infra.Interfaces
{
    public interface IUnitOfWork
    {
        Task CommitAsync(CancellationToken cancellationToken);
        Task RollbackAsync();
    }
}
