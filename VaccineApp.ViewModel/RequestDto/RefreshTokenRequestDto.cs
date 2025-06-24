using System.Text.Json.Serialization;

namespace VaccineApp.ViewModel.RequestDto
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; }
        public Guid UserId { get; set; }

        [JsonIgnore]
        public string? Ip {  get; set; }
    }
}
