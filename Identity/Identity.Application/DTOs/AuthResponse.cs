namespace Identity.Application.DTOs
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; init; } = null!;
        public string RefreshToken { get; init; } = null!;
        public DateTime ExpiresAt { get; init; }
        public List<string> Roles { get; init; } = new();
    }
}
