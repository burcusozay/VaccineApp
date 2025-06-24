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
using VaccineApp.ViewModel.RequestDto;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace VaccineApp.Business.Services
{
    public class FreezerService : BaseService<Freezer, FreezerDto>, IFreezerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Freezer, long> _freezerRepository;
        private readonly IOutboxMessageService _outboxMessageService;

        public FreezerService(IUnitOfWork unitOfWork, IMapper mapper, IOutboxMessageService outboxService)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _freezerRepository = _unitOfWork.GetRepository<Freezer, long>();
            _outboxMessageService = outboxService;
        }

        public async Task<IEnumerable<FreezerDto>> GetAllFreezersAsync()
        {
            var freezerList = await _freezerRepository.GetAllAsync();
            return MapToDtoList(freezerList);
        }

        public async Task<ServiceResponseDto<FreezerDto>> GetFreezerListAsync(FreezerRequestDto model)
        {
            var query = _freezerRepository.AsQueryable()
                .Where(x => !x.IsDeleted)
                .WhereIf(model.Name.IsNotNullOrEmpty(), x => x.Name.StartsWith(model.Name.Trim()));

            // 2. Sayfalama yapmadan ÖNCE toplam kayıt sayısını al.
            var totalCount = await query.CountAsync();

            // 3. Sayfalama ve sıralamayı uygula.
            var pagedQuery = query
                .OrderByDescending(x => x.CreatedDate).Select(x => x);


            if (model.Page.HasValue && model.PageSize.HasValue)
            {
                pagedQuery = pagedQuery.Skip((model.Page.Value - 1) * model.PageSize.Value) // Sayfa numarasını ve boyutunu kullan
                .Take(model.PageSize.Value);
            }


            // 4. Veritabanından sadece ilgili sayfadaki veriyi çek.
            var pagedItems = await pagedQuery.ToListAsync();

            var items = MapToDtoList(pagedItems).ToList();

            return new ServiceResponseDto<FreezerDto>(items, totalCount, model.Page, model.PageSize);

        }

        public async Task<FreezerDto?> GetFreezerByIdAsync(long id)
        {
            var freezerDto = await _freezerRepository.GetByIdAsync(id);
            if (freezerDto is null) return null;
            return MapToDto(freezerDto);
        }

        public async Task<FreezerDto> AddFreezerAsync(FreezerDto model)
        {
            var freezerEntity = MapToEntity(model);
            freezerEntity = await _freezerRepository.InsertAsync(freezerEntity); 
            return MapToDto(freezerEntity); ;
        }

        public async Task<FreezerDto?> UpdateFreezerAsync(long id, FreezerDto model)
        {
            var existing = await _freezerRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.Name= model.Name;
            existing.OrderNo = model.OrderNo;
            existing.CreatedDate = model.CreatedDate.IsNotNullOrEmpty() ? model.CreatedDate : DateTime.UtcNow; // If CreatedDate is null, use the current date
            existing.IsDeleted = model.IsDeleted;
            existing.IsActive = model.IsActive; // Aktiflik durumu   u 

            await _freezerRepository.UpdateAsync(existing);

            var freezerDto = MapToDto(existing);
            return freezerDto;
        }

        public async Task<bool> DeleteFreezerAsync(long id)
        {
            var existing = await _freezerRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _freezerRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<bool> SoftDeleteFreezerAsync(long id)
        {
            await _freezerRepository.SoftDeleteAsync(id);
            return true;
        }
    }
}
