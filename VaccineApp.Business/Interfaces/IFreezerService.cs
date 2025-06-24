using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IFreezerService
    {
        Task<IEnumerable<FreezerDto>> GetAllFreezersAsync();
        Task<ServiceResponseDto<FreezerDto>> GetFreezerListAsync(FreezerRequestDto model);
        Task<FreezerDto?> GetFreezerByIdAsync(long id);
        Task<FreezerDto> AddFreezerAsync(FreezerDto temprature);
        Task<FreezerDto?> UpdateFreezerAsync(long id, FreezerDto updated);
        Task<bool> DeleteFreezerAsync(long id);
        Task<bool> SoftDeleteFreezerAsync(long id);
    }
}
