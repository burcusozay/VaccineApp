using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;
using VaccineApp.ViewModel.Options;

namespace VaccineApp.Business.Services
{
    public class AccountService : IAccountService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDistributedCache _redis;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly JWTSettingOptions _jwtSettings;
        private readonly IRepository<RefreshToken, long> _refreshTokenRepo;
        public AccountService(IUnitOfWork unitOfWork, IOptions<JWTSettingOptions> jwtSetting, IDistributedCache redis, IHttpContextAccessor httpContext,
            IUserService userService,
            ILogger<AuditService> logger)
        {
            _logger = logger;
            _jwtSettings = jwtSetting.Value;
            _redis = redis;
            _httpContext = httpContext;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _refreshTokenRepo =_unitOfWork.GetRepository<RefreshToken, long>();
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userService.VerifyUser(new UserCheckDto() { Username = request.Username, Password = request.Password });
            if (user == null)
                return null;

            var accessToken = await CreateAccessTokenAsync(user);
            var refreshToken = await CreateRefreshTokenAsync(new RefreshTokenRequestDto()
            {
                AccessToken = accessToken,
                UserId = user.Id,
                Ip = _httpContext.HttpContext.Connection.RemoteIpAddress.ToString() ?? "local"
            });

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.RefreshToken,
                ExpirationTime = refreshToken.Expires,
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<string> CreateAccessTokenAsync(UserDto user)
        {
            var token = await GenerateAccessTokenAsync(user);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshTokenDto?> CreateRefreshTokenAsync(RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.AccessToken) || request.UserId <= 0)
                return null;

            // Yeni token üret
            var user = await _userService.GetUserByIdAsync(request.UserId);
            if (user == null)
                return null;

            request.Ip = _httpContext.HttpContext.Connection.RemoteIpAddress.ToString() ?? "local";
            request.UserId = user.Id; 

            var existingToken = await ValidateRefreshTokenAsync(request);
            if (existingToken != null && existingToken.CreatedByIp != request.Ip)
                return null;

            var newRefreshToken = await GenerateRefreshTokenAsync();
            newRefreshToken.AccessToken = request.AccessToken;
            newRefreshToken.UserId = user.Id;

            // Yeni refresh token'ı kaydet
            await StoreRefreshTokenAsync(newRefreshToken);


            return new RefreshTokenDto()
            {
                RefreshToken = newRefreshToken.RefreshToken,
                AccessToken = request.AccessToken.ToString(),
                CreatedByIp = _httpContext.HttpContext.Connection.RemoteIpAddress.ToString() ?? "local",
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                UserId = user.Id,
                Created = DateTime.UtcNow,
                ReplacedByToken = existingToken?.RefreshToken,
            };
        }

        // Refresh token geçerli mi kontrol et
        public async Task<RefreshTokenDto?> ValidateRefreshTokenAsync(RefreshTokenRequestDto refreshToken)
        {
            var key = $"refresh:{refreshToken.UserId}:{refreshToken.AccessToken}";
            var json = await _redis.GetStringAsync(key);
            if (string.IsNullOrEmpty(json))
                return null;

            var token = System.Text.Json.JsonSerializer.Deserialize<RefreshTokenDto>(json);
            if (token == null || token.IsExpired)
                return null;

            return token;
        }

        // Refresh token Redis'e kaydet
        public async Task StoreRefreshTokenAsync(RefreshTokenDto refreshToken)
        {
            await SaveRefreshTokenToDbAsync(refreshToken);

            var key = $"refresh:{refreshToken.UserId}:{refreshToken.AccessToken}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = refreshToken.Expires
            };

            var json = System.Text.Json.JsonSerializer.Serialize(refreshToken);
            await _redis.SetStringAsync(key, json, options);
        }

        public async Task InvalidateRefreshTokenAsync(RefreshTokenRequestDto refreshToken)
        {
            var key = $"refresh:{refreshToken.UserId}:{refreshToken.AccessToken}";
            await _redis.RemoveAsync(key);
        }

        private async Task<RefreshTokenDto> GenerateRefreshTokenAsync()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new RefreshTokenDto
            {
                RefreshToken = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedByIp = _httpContext.HttpContext.Connection.RemoteIpAddress.ToString() ?? "local"
            };
        }

        private async Task<JwtSecurityToken> GenerateAccessTokenAsync(UserDto user)
        {
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
            };

            return new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );
        }
        private async Task SaveRefreshTokenToDbAsync(RefreshTokenDto dto)
        {
            var entity = new RefreshToken
            {
                UserId = dto.UserId,
                Token = dto.RefreshToken,
                Created = DateTime.UtcNow,
                CreatedByIp = dto.CreatedByIp,
                Expires = dto.Expires,
                ReplacedByToken = dto.ReplacedByToken,
                Revoked = dto.Revoked,
                RevokedByIp = dto.RevokedByIp
            };

            await _refreshTokenRepo.InsertAsync(entity);
        }

    }
}