namespace VaccineApp.Core
{
    public interface IBaseEntityDto<TKey>
    {
        TKey Id { get; set; }
    }
}
