using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VaccineApp.Business.Interfaces;
using VaccineApp.Data.Context;
using VaccineApp.Data.Entities;

namespace VaccineApp.Business.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<AuditService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task SaveAuditLogsAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
            var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var controller = httpContext?.GetRouteValue("controller")?.ToString();
            var action = httpContext?.GetRouteValue("action")?.ToString();
            var route = httpContext?.Request?.Path;

            var entries = _context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

            var logs = new List<AuditLog>();

            foreach (var entry in entries)
            {
                var log = new AuditLog
                {
                    Username = username,
                    IpAddress = ip,
                    Action = entry.State.ToString(),
                    Controller = controller,
                    ActionName = action,
                    Route = route,
                    TableName = entry.Entity.GetType().Name,
                    PrimaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString()
                };

                var changes = new Dictionary<string, object>();

                foreach (var prop in entry.Properties)
                {
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            if (!Equals(prop.OriginalValue, prop.CurrentValue))
                            {
                                changes.Add(prop.Metadata.Name, new
                                {
                                    Old = prop.OriginalValue,
                                    New = prop.CurrentValue
                                });
                            }
                            break;
                        case EntityState.Added:
                            changes.Add(prop.Metadata.Name, new { New = prop.CurrentValue });
                            break;
                        case EntityState.Deleted:
                            changes.Add(prop.Metadata.Name, new { Old = prop.OriginalValue });
                            break;
                    }
                }

                log.Changes = JsonSerializer.Serialize(changes);
                logs.Add(log);
            }

            if (logs.Any())
            {
                await _context.AuditLogs.AddRangeAsync(logs);
                _logger.LogInformation("Audit log kaydedildi: {@Logs}", logs);
            }
        }
    } 
}
