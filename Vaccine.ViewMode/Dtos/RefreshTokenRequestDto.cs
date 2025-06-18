using System.Text.Json.Serialization;

namespace VaccineApp.ViewModel.Dtos
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; }
        public long UserId { get; set; }

        [JsonIgnore]
        public string? Ip {  get; set; }
    }
}
