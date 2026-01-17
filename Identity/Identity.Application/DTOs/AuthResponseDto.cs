public class AuthResponseDto
{
    public Guid UserId { get; init; }
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public IEnumerable<string> Roles { get; init; } = new List<string>();

 
}
