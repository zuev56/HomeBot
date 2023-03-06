using System.Text.Json.Serialization;

namespace HomeBot.Features.UserWatcher;

internal sealed class User
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = null!;
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = null!;
}