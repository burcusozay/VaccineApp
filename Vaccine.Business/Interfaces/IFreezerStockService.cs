using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IFreezerStockService
    {
        Task<IEnumerable<FreezerStockDto>> GetAllStocksAsync();
        Task<FreezerStockDto?> GetStockByIdAsync(int id);
        Task<FreezerStockDto> AddStockAsync(FreezerStockDto stock);
        Task<FreezerStockDto?> UpdateStockAsync(int id, FreezerStockDto updated);
        Task<bool> DeleteStockAsync(int id);
    }
}
