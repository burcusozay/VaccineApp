namespace VaccineApp.ViewModel.Dtos
{
    public class UserRoleDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; } 
          
        public Guid RoleId { get; set; }  

        public DateTime CreatedDate { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
     }
}
