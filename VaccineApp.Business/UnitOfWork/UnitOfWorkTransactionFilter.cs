using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using VaccineApp.Business.UnitOfWork;

namespace VaccineApp.Business.UnitOfWork
{
    public class UnitOfWorkTransactionFilter : IAsyncActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UnitOfWorkTransactionFilter> _logger;

        public UnitOfWorkTransactionFilter(IUnitOfWork unitOfWork, ILogger<UnitOfWorkTransactionFilter> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpMethod = context.HttpContext.Request.Method;

            var shouldUseTransaction = !HttpMethods.IsGet(httpMethod); // alternatif kontrol

            if (shouldUseTransaction)
            {
                await _unitOfWork.BeginTransactionAsync();
            }

            var executedContext = await next();

            if (executedContext.Exception is null || executedContext.ExceptionHandled)
            {
                if (shouldUseTransaction)
                {
                    try
                    {
                        await _unitOfWork.CommitAsync();
                        _logger.LogInformation("Transaction committed successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Commit failed.");
                        await _unitOfWork.RollbackAsync();
                        throw;
                    }
                }
            }
            else
            {
                if (shouldUseTransaction)
                {
                    _logger.LogWarning("Transaction rollback due to exception.");
                    await _unitOfWork.RollbackAsync();
                }
            }
        }

    }
}