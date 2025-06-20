using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using VaccineApp.Business.Base;
using VaccineApp.Business.Interfaces;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;

namespace VaccineApp.Business.Services
{
    public class UserService : BaseService<User, UserDto>, IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, long> _userRepo;
        private readonly IPasswordHasher<User> _passwordHasher;
        public UserService(IUnitOfWork unitOfWork, IPasswordHasher<User> passwordHasher, ILogger<UserService> logger, IMapper mapper) : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _userRepo = _unitOfWork.GetRepository<User, long>();
        }

        public async Task<UserDto> GetUserByIdAsync(long id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
            {
                throw new Exception($"User bulunamadı");
            }

            return MapToDto(user);
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepo.FirstOrDefaultAsync(x => x.Username == username);
            if (user == null)
            {
                throw new Exception($"{username} User bulunamadı");
            }

            return MapToDto(user);
        }

        public async Task<UserDto> VerifyUser(UserCheckDto model)
        {
            var user = await _userRepo.FirstOrDefaultAsync(x => x.Username == model.Username);
            if (user == null)
            {
                throw new Exception($"{model.Username} User bulunamadı");
            }
            var userPassHash = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (userPassHash == PasswordVerificationResult.Success)
            {
               return MapToDto(user);
            }

            return null;
        }

        public Task<List<UserDto>> GetUserListAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}
