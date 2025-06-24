using VaccineApp.Core.Core;

namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerRequestDto : BaseRequestDto
    {
        public string? Name { get; set; } = null!;

        public int? OrderNo { get; set; }
    }
}
