using System.Linq.Expressions;
using VaccineApp.Core;

namespace VaccineApp.Business.Repository
{
    public interface IRepository<T, TKey> where T : BaseEntity<TKey>
    {
        Task<T?> GetByIdAsync(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> InsertAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveChangesAsync();

        IQueryable<T> AsQueryable();
        // Alternatif olarak, filtreli bir IQueryable döndürmek isterseniz:
        IQueryable<T> AsQueryable(Expression<Func<T, bool>> predicate);


    }
}
