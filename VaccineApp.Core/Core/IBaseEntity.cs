namespace VaccineApp.Core
{
    public interface IBaseEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
