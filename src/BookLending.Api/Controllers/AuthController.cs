using BookLending.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BookLending.Api.Controllers;

[ApiController]
[Route("login")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    public IActionResult Login(LoginRequest request)
    {
        var response = authService.Login(request);
        return response is null ? Unauthorized() : Ok(response);
    }
}
