using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VaccineApp.Core;
using VaccineApp.Data.Context;

namespace VaccineApp.Business.Repository
{
    public class Repository<T, TKey> :  IRepository<T, TKey> where T : BaseEntity<TKey>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _entities;

        public Repository(AppDbContext context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(TKey id)
        {
            return await _entities.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _entities.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _entities.Where(predicate).ToListAsync();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _entities.FirstOrDefaultAsync(predicate);
        }

        public async Task<T> InsertAsync(T entity)
        {
            await _entities.AddAsync(entity);
            return entity;
        }

        public async Task UpdateAsync(T entity)
        {
            _entities.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(T entity)
        {
            _entities.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
