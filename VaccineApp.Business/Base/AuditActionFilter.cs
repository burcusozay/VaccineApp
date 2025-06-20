using Microsoft.AspNetCore.Mvc.Filters;
using VaccineApp.Business.Interfaces;
using VaccineApp.Data.Context;

namespace VaccineApp.Business.Base
{
    public class AuditActionFilter : IAsyncActionFilter
    {
        private readonly IAuditService _auditService;
        private readonly AppDbContext _context;
        public AuditActionFilter(IAuditService auditService, AppDbContext context)
        {
            _auditService = auditService;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Exception == null || resultContext.ExceptionHandled)
            {
                await _auditService.SaveAuditLogsAsync();
                await _context.SaveChangesAsync();
            }
        }
    }
}