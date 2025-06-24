namespace VaccineApp.Core.Core
{
    [Serializable]
    public abstract class BaseRequestDto
    {  
        public int? PageSize { get; set; }
        public int? Page { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true; 
    }
}
