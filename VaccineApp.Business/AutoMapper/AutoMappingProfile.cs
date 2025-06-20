using AutoMapper;
using VaccineApp.Data.Entities;
using VaccineApp.ViewModel.Dtos; 

namespace VaccineApp.Business.AutoMapper
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<AuditLog, AuditLogDto>().ReverseMap();
            CreateMap<FreezerStock, FreezerStockDto>().ReverseMap();
            CreateMap<Freezer, FreezerDto>().ReverseMap();
            CreateMap<Vaccine, VaccineDto>().ReverseMap();
            CreateMap<VaccineOrder, VaccineOrderDto>().ReverseMap();
            CreateMap<VaccineFreezer, VaccineFreezerDto>().ReverseMap();
            CreateMap<FreezerTemprature, FreezerTempratureDto>().ReverseMap();
            // Diğer entity <-> dto eşlemeleri de buraya eklenebilir
        }
    }
}