using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class RoleDto : BaseEntityDto<long>
    {  
        public string Name { get; set; } = null!;
          
        public string Normalizedname { get; set; } = null!;  

        public string Description { get; set; } = null!; 
    }
}
