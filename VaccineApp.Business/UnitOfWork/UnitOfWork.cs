using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VaccineApp.Core;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Data.Context;

namespace VaccineApp.Business.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly IDistributedCache _cache;
        private readonly IAuditService _auditService;
        private readonly Dictionary<string, object> _repositories = new();
          
        public UnitOfWork(AppDbContext context, IDistributedCache cache, IAuditService auditService)
        {
            _context = context;
            _cache = cache;
            _auditService = auditService;
        } 

        public IRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : BaseEntity<TKey>
        {
            var typeName = typeof(TEntity).Name;

            if (_repositories.ContainsKey(typeName))
                return (IRepository<TEntity, TKey>)_repositories[typeName];

            var repositoryInstance = new Repository<TEntity, TKey>(_context);
            _repositories[typeName] = repositoryInstance;

            return repositoryInstance;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            if (_transaction is not null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction is not null)
            {
                await _auditService.SaveAuditLogsAsync(); // Audit kayıtla
                await _context.SaveChangesAsync();        // AuditLog dahil her şeyi kaydet
                await _transaction.CommitAsync();
                await SyncRedisCacheAsync(); // Redis güncelleme
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

        private async Task SyncRedisCacheAsync()
        {
            // Örnek: tüm FreezerStock'ları cache’e at
            var stocks = await _context.FreezerStocks.ToListAsync();
            foreach (var stock in stocks)
            {
                var key = $"stock_{stock.Id}";
                var json = JsonSerializer.Serialize(stock);
                await _cache.SetStringAsync(key, json);
            }
        }
    }
}