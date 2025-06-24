using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;
using VaccineApp.ViewModel.RequestDto;

namespace VaccineApp.Business.Interfaces
{
    public interface IAccountService
    {  
        Task<string> CreateAccessTokenAsync(UserDto user);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto user);
        Task<RefreshTokenDto?> CreateRefreshTokenAsync(RefreshTokenRequestDto token);
        Task<RefreshTokenDto?> ValidateRefreshTokenAsync(RefreshTokenRequestDto token);
        Task InvalidateRefreshTokenAsync(RefreshTokenRequestDto token);
        Task StoreRefreshTokenAsync(RefreshTokenDto refreshToken);
    }
}
