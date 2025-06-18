using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IUserService
    { 
        Task<UserDto> GetUserByIdAsync(long id);
        Task<UserDto> VerifyUser(UserCheckDto model);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<List<UserDto>> GetUserListAsync(string token);
    }
}
