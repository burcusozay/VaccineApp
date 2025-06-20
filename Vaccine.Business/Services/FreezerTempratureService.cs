using AutoMapper;
using VaccineApp.Business.Base;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Services
{
    public class FreezerTempratureService : BaseService<FreezerTemprature, FreezerTempratureDto>, IFreezerTempratureService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly IRepository<FreezerTemprature, long> _freezerTempratureRepository;

        public FreezerTempratureService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _freezerTempratureRepository = _unitOfWork.GetRepository<FreezerTemprature, long>();
        }

        public async Task<IEnumerable<FreezerTempratureDto>> GetAllTempraturesAsync()
        {
            var tempratureList = await _freezerTempratureRepository.GetAllAsync();
            return  MapToDtoList(tempratureList);
        }

        public async Task<FreezerTempratureDto?> GetTempratureByIdAsync(int id)
        {
            var tempratureDto = await _freezerTempratureRepository.GetByIdAsync(id);
            return MapToDto(tempratureDto);
        }

        public async Task<FreezerTempratureDto> AddTempratureAsync(FreezerTempratureDto model)
        {
            var tempratureEntity = MapToEntity(model);
            tempratureEntity = await _freezerTempratureRepository.InsertAsync(tempratureEntity);
            await _unitOfWork.SaveChangesAsync();
            return  MapToDto(tempratureEntity); ;
        }

        public async Task<FreezerTempratureDto?> UpdateTempratureAsync(int id, FreezerTempratureDto model)
        {
            var existing = await _freezerTempratureRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.Temprature = model.Temprature;
            // diğer alanlar güncellenebilir...

            await _unitOfWork.SaveChangesAsync();

            var tempratureDto =  MapToDto(existing);

            return tempratureDto;
        }

        public async Task<bool> DeleteTempratureAsync(int id)
        {
            var existing = await _freezerTempratureRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _freezerTempratureRepository.DeleteAsync(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
