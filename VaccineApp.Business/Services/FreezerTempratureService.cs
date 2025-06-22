using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VaccineApp.Business.Base;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Services
{
    public class FreezerTemperatureService : BaseService<FreezerTemperature, FreezerTemperatureDto>, IFreezerTemperatureService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<FreezerTemperature, long> _freezerTemperatureRepository;
        private readonly IOutboxMessageService _outboxMessageService;

        public FreezerTemperatureService(IUnitOfWork unitOfWork, IMapper mapper, IOutboxMessageService outboxService)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _freezerTemperatureRepository = _unitOfWork.GetRepository<FreezerTemperature, long>();
            _outboxMessageService = outboxService;
        }

        public async Task<IEnumerable<FreezerTemperatureDto>> GetAllTemperaturesAsync()
        {
            var tempratureList = await _freezerTemperatureRepository.GetAllAsync();
            return MapToDtoList(tempratureList);
        }

        public async Task<FreezerTemperatureDto?> GetTemperatureByIdAsync(int id)
        {
            var tempratureDto = await _freezerTemperatureRepository.GetByIdAsync(id);
            return MapToDto(tempratureDto);
        }

        public async Task<FreezerTemperatureDto> AddTemperatureAsync(FreezerTemperatureDto model)
        {
            var tempratureEntity = MapToEntity(model);
            tempratureEntity = await _freezerTemperatureRepository.InsertAsync(tempratureEntity);

            var outboxMsg = new OutboxMessageDto
            {
                Type = "FreezerAlarm",
                Payload = JsonSerializer.Serialize(new
                {
                    FreezerId = tempratureEntity.FreezerId,
                    Temperature = tempratureEntity.Temperature,
                    CreatedAt = tempratureEntity.CreatedDate
                }),
                OccuredOn = DateTime.UtcNow
            };

            await _outboxMessageService.AddOutboxMessageAsync(outboxMsg);

            return MapToDto(tempratureEntity); ;
        }

        public async Task<FreezerTemperatureDto?> UpdateTemperatureAsync(int id, FreezerTemperatureDto model)
        {
            var existing = await _freezerTemperatureRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.Temperature = model.Temperature;
            // diğer alanlar güncellenebilir...

            await _unitOfWork.SaveChangesAsync();

            var tempratureDto = MapToDto(existing);

            return tempratureDto;
        }

        public async Task<bool> DeleteTemperatureAsync(int id)
        {
            var existing = await _freezerTemperatureRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _freezerTemperatureRepository.DeleteAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
