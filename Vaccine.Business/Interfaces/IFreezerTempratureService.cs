using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IFreezerTempratureService
    {
        Task<IEnumerable<FreezerTempratureDto>> GetAllTempraturesAsync();
        Task<FreezerTempratureDto?> GetTempratureByIdAsync(int id);
        Task<FreezerTempratureDto> AddTempratureAsync(FreezerTempratureDto temprature);
        Task<FreezerTempratureDto?> UpdateTempratureAsync(int id, FreezerTempratureDto updated);
        Task<bool> DeleteTempratureAsync(int id);
    }
}
