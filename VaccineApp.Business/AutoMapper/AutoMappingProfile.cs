using AutoMapper;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos;
using VaccineApp.ViewModel.RequestDto;

namespace VaccineApp.Business.AutoMapper
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<User, UserDto>()
                  .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.UserRoles.Select(x => x.Role.Name).FirstOrDefault()))
                  .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()));
            CreateMap<UserDto, User>();
            CreateMap<UserRole, UserRoleDto>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<AuditLog, AuditLogDto>().ReverseMap();
            CreateMap<FreezerStock, FreezerStockDto>().ReverseMap();
            CreateMap<Freezer, FreezerDto>().ReverseMap();
            CreateMap<Vaccine, VaccineDto>().ReverseMap();
            CreateMap<VaccineOrder, VaccineOrderDto>().ReverseMap();
            CreateMap<VaccineFreezer, VaccineFreezerDto>().ReverseMap();
            CreateMap<FreezerTemperature, FreezerTemperatureDto>().ReverseMap();
            CreateMap<FreezerTemperature, FreezerTemperatureDto>().ReverseMap();
            CreateMap<OutboxMessage, OutboxMessageDto>().ReverseMap();
            // Diğer entity <-> dto eşlemeleri de buraya eklenebilir
        }
    }
}