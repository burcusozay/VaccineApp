using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class VaccineDto : BaseEntityDto<long>
    {  
        public string Name { get; set; } = null!; 

        public string CompanyName { get; set; } = null!;
    }
}
