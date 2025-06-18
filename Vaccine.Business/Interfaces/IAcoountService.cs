using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Interfaces
{
    public interface IAccountService
    {  
        Task<string> CreateAccessTokenAsync(UserDto user);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto user);
        Task<RefreshTokenDto?> CreateRefreshTokenAsync(RefreshTokenRequestDto token);
        Task<RefreshTokenDto?> ValidateRefreshTokenAsync(long userId, string refreshToken);
        Task InvalidateRefreshTokenAsync(long userId, string refreshToken);
        Task StoreRefreshTokenAsync(long userId, RefreshTokenDto refreshToken);
    }
}
