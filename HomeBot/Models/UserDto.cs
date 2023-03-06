using System.Text.Json.Serialization;

namespace HomeBot.Models;

public sealed class UserDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = null!;
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = null!;
}