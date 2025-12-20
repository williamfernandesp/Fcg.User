using System.Text.Json.Serialization;

namespace Fcg.User.Application.Handlers
{
    public class CreateUserResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }
}
