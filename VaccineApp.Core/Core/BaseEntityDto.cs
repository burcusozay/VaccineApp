using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaccineApp.Core
{
    [Serializable]
    public abstract class BaseEntityDto<TKey> : IBaseEntityDto<TKey>
    { 
        public TKey Id { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } 
    }
}
