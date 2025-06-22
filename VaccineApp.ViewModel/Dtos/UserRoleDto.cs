using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class UserRoleDto : BaseEntityDto<Guid>
    {  
        public Guid UserId { get; set; } 
          
        public Guid RoleId { get; set; }   
     }
}
