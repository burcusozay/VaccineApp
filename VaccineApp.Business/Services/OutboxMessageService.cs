using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VaccineApp.Business.Base;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Services
{
    public class OutboxMessageService : BaseService<OutboxMessage, OutboxMessageDto>, IOutboxMessageService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly IRepository<OutboxMessage, long> _outboxMessageRepository;

        public OutboxMessageService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _outboxMessageRepository = _unitOfWork.GetRepository<OutboxMessage, long>();
        }

        public async Task<IEnumerable<OutboxMessageDto>> GetAllOutboxMessagesAsync()
        {
            var outboxMessages = await _outboxMessageRepository.GetAllAsync();
            return  MapToDtoList(outboxMessages);
        }

        public async Task<OutboxMessageDto?> GetOutboxMessageByIdAsync(long id)
        {
            var outboxMessageDto = await _outboxMessageRepository.GetByIdAsync(id);
            return MapToDto(outboxMessageDto);
        }

        public async Task<OutboxMessageDto> AddOutboxMessageAsync(OutboxMessageDto model)
        {
            var outboxMessageEntity = MapToEntity(model);
            outboxMessageEntity = await _outboxMessageRepository.InsertAsync(outboxMessageEntity);
            return  MapToDto(outboxMessageEntity); ;
        }

        public async Task<List<OutboxMessageDto>> GetUnprocessedMessageListAsync()
        {
            var messages = await _outboxMessageRepository.AsQueryable()
               .Where(x => x.ProcessedOn == null)
               .OrderBy(x => x.OccuredOn)
               .Take(20)
               .ToListAsync();

            // Entity → DTO map (ör: Automapper ile)
            var dtoList = _mapper.Map<List<OutboxMessageDto>>(messages);
            return dtoList;
        }

        public async Task<OutboxMessageDto?> MarkProcessedMessageAsync(long id)
        {
            var existing = await _outboxMessageRepository.GetByIdAsync(id);
            if (existing is null) return null;

            if (existing.ProcessedOn != null)
            {
                return null;
            }
            // TODO: sonradan bakılacak
            existing.ProcessedOn = DateTime.UtcNow;
            existing.IsActive = false;
            // diğer alanlar güncellenebilir...

            await _outboxMessageRepository.UpdateAsync(existing); 

            var outboxMessageDto =  MapToDto(existing); 
            return outboxMessageDto;
        }

        public async Task<bool> DeleteStockAsync(int id)
        {
            var existing = await _outboxMessageRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _outboxMessageRepository.DeleteAsync(existing); 
            return true;
        }
    }
}
