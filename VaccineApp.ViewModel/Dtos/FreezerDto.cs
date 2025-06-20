namespace VaccineApp.ViewModel.Dtos
{
    public class FreezerDto
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public int OrderNo { get; set; }

    }
}
