using VaccineApp.Core;
using VaccineApp.Business.Repository;

namespace VaccineApp.Business.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {  
        Task<int> SaveChangesAsync();

        // Transaction kontrolü
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>;
    }
}
