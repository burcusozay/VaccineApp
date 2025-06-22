using AutoMapper;
using VaccineApp.Business.Base;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Services
{
    public class FreezerStockService : BaseService<FreezerStock, FreezerStockDto>, IFreezerStockService
    {
        private readonly IUnitOfWork _unitOfWork; 
        private readonly IRepository<FreezerStock, long> _freezerStockRepository;

        public FreezerStockService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _freezerStockRepository = _unitOfWork.GetRepository<FreezerStock, long>();
        }

        public async Task<IEnumerable<FreezerStockDto>> GetAllStocksAsync()
        {
            var stockList = await _freezerStockRepository.GetAllAsync();
            return  MapToDtoList(stockList);
        }

        public async Task<FreezerStockDto?> GetStockByIdAsync(int id)
        {
            var stockDto = await _freezerStockRepository.GetByIdAsync(id);
            return MapToDto(stockDto);
        }

        public async Task<FreezerStockDto> AddStockAsync(FreezerStockDto model)
        {
            var stockEntity = MapToEntity(model);
            stockEntity = await _freezerStockRepository.InsertAsync(stockEntity); 
            return  MapToDto(stockEntity); ;
        }

        public async Task<FreezerStockDto?> UpdateStockAsync(int id, FreezerStockDto model)
        {
            var existing = await _freezerStockRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.StockCount = model.StockCount;
            // diğer alanlar güncellenebilir...

            await _freezerStockRepository.UpdateAsync(existing);

            var stockDto =  MapToDto(existing);

            return stockDto;
        }

        public async Task<bool> DeleteStockAsync(int id)
        {
            var existing = await _freezerStockRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _freezerStockRepository.DeleteAsync(existing); 
            return true;
        }
    }
}
