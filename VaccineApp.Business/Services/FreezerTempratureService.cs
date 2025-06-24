using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using VaccineApp.Business.Base;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public async Task<ServiceResponseDto<FreezerTemperatureDto>> GetFreezerTemperatureListAsync(FreezerTemperatureRequestDto model)
        {
            var query = _freezerTemperatureRepository.AsQueryable()
                .Where(x => !x.IsDeleted)
                .WhereIf(model.StartDate.HasValue, x => x.CreatedDate >= model.StartDate)
                .WhereIf(model.EndDate.HasValue, x => x.CreatedDate <= model.EndDate)
                .WhereIf(model.MinValue.HasValue && model.MinValue.Value > 0, x => x.Temperature >= model.MinValue)
                .WhereIf(model.MaxValue.HasValue && model.MaxValue > 0, x => x.Temperature <= model.MaxValue);

            // 2. Sayfalama yapmadan ÖNCE toplam kayıt sayısını al.
            var totalCount = await query.CountAsync();

            // 3. Sayfalama ve sıralamayı uygula.
            var pagedQuery = query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((model.Page - 1) * model.PageSize) // Sayfa numarasını ve boyutunu kullan
                .Take(model.PageSize);

            // 4. Veritabanından sadece ilgili sayfadaki veriyi çek.
            var pagedItems = await pagedQuery.ToListAsync();

            var items = MapToDtoList(pagedItems).ToList();

            return new ServiceResponseDto<FreezerTemperatureDto>(items, totalCount, model.Page, model.PageSize);

        }

        public async Task<FreezerTemperatureDto?> GetTemperatureByIdAsync(long id)
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

        public async Task<FreezerTemperatureDto?> UpdateTemperatureAsync(long id, FreezerTemperatureDto model)
        {
            var existing = await _freezerTemperatureRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.Temperature = model.Temperature;
            existing.FreezerId = model.FreezerId;
            existing.CreatedDate = model.CreatedDate.IsNotNullOrEmpty() ? model.CreatedDate : DateTime.UtcNow; // If CreatedDate is null, use the current date
            existing.IsDeleted = model.IsDeleted;
            existing.IsActive = model.IsActive; // Aktiflik durumu   u 

            await _freezerTemperatureRepository.UpdateAsync(existing);

            var tempratureDto = MapToDto(existing);
            return tempratureDto;
        }

        public async Task<bool> DeleteTemperatureAsync(int id)
        {
            var existing = await _freezerTemperatureRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _freezerTemperatureRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<bool> SoftDeleteTemperatureAsync(int id)
        {
            await _freezerTemperatureRepository.SoftDeleteAsync(id);
            return true;
        }
    }
}
