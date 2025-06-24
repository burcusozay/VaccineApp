using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IFreezerTemperatureService
    {
        Task<IEnumerable<FreezerTemperatureDto>> GetAllTemperaturesAsync();
        Task<ServiceResponseDto<FreezerTemperatureDto>> GetFreezerTemperatureListAsync(FreezerTemperatureRequestDto model);
        Task<FreezerTemperatureDto?> GetTemperatureByIdAsync(long id);
        Task<FreezerTemperatureDto> AddTemperatureAsync(FreezerTemperatureDto temprature);
        Task<FreezerTemperatureDto?> UpdateTemperatureAsync(long id, FreezerTemperatureDto updated);
        Task<bool> DeleteTemperatureAsync(int id);
        Task<bool> SoftDeleteTemperatureAsync(int id);
    }
}
