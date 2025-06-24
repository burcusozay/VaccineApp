using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaccineApp.Core
{
    [Serializable]
    public abstract class BaseRequestDto
    {  
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true; 
    }
}
