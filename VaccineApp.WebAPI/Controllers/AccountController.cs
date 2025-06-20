using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaccineApp.Business.Interfaces;
using VaccineApp.ViewModel.Dtos;
using VaccineApp.ViewModel.Options;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;
        private readonly IUserService _userService;
        private readonly JWTSettingOptions _jwtSettings;
        //private readonly IPasswordHasher<UserDto> _passwordHasher;
        public AccountController(ILogger<AccountController> logger, IAccountService accountService, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
            _accountService = accountService;
            //_passwordHasher = passwordHasher;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _accountService.LoginAsync(request);
            if (response == null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı.");

            return Ok(response);

        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var newRefreshToken = await _accountService.CreateRefreshTokenAsync(request);
            if (newRefreshToken == null)
            {
                return Unauthorized(new { message = "Refresh token geçersiz veya süresi dolmuş." });
            }

            return Ok(newRefreshToken);
        }
    }
}