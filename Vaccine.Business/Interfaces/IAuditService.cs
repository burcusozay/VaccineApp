using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IAuditService
    { 
        Task SaveAuditLogsAsync();
    }
}
