namespace BookLending.Application.Auth;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, DateTime ExpiresAt);

public interface IAuthService
{
    public LoginResponse? Login(LoginRequest request);
}
