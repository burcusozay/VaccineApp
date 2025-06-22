using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IOutboxMessageService
    {
        Task<IEnumerable<OutboxMessageDto>> GetAllOutboxMessagesAsync();
        Task<OutboxMessageDto?> GetOutboxMessageByIdAsync(long id);
        Task<OutboxMessageDto> AddOutboxMessageAsync(OutboxMessageDto message);
        Task<List<OutboxMessageDto>> GetUnprocessedMessageListAsync();
        Task<OutboxMessageDto?> MarkProcessedMessageAsync(long id);
        
    }
}
