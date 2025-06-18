using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VaccineApp.Business.Interfaces;
using VaccineApp.Data.Context;

namespace VaccineApp.Business.UnitOfWork
{
    public class TransactionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TransactionMiddleware> _logger;

        public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, IAuditService auditService)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                await _next(context); // diğer middleware/controller çağrılır

                if (context.Response.StatusCode < 400) // başarılıysa
                {
                    await auditService.SaveAuditLogsAsync(); // audit kaydı
                    await dbContext.SaveChangesAsync();      // audit DB'ye kaydedilir
                    await transaction.CommitAsync();         // veritabanı commit
                }
                else
                {
                    await transaction.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction rollback yapıldı.");
                throw;
            }
        }
    }

} 