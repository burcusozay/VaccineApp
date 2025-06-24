using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IFreezerTemperatureService
    {
        Task<IEnumerable<FreezerTemperatureDto>> GetAllTemperaturesAsync();
        Task<ServiceResponseDto<FreezerTemperatureDto>> GetFreezerTemperaturesAsync(FreezerTemperatureRequestDto model);
        Task<FreezerTemperatureDto?> GetTemperatureByIdAsync(int id);
        Task<FreezerTemperatureDto> AddTemperatureAsync(FreezerTemperatureDto temprature);
        Task<FreezerTemperatureDto?> UpdateTemperatureAsync(int id, FreezerTemperatureDto updated);
        Task<bool> DeleteTemperatureAsync(int id);
    }
}
