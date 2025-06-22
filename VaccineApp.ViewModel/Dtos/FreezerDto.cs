using VaccineApp.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerDto : BaseEntity<long>
    {
        public string Name { get; set; } = null!;

        public int OrderNo { get; set; }
    }
}
