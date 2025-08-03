using Clarity.Domain.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public record RegisterDto(string Email, string Username, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token);


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<AuthController> logger) 
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger; 
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var user = new ApplicationUser
        {
            UserName = registerDto.Username,
            Email = registerDto.Email
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { Message = "User registered successfully!" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return Unauthorized(new { Message = "Invalid credentials." });
        }

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto(token));
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id), 
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
        };


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), 
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Authorize]
    [HttpGet("connect-google-calendar")]
    public IActionResult ConnectGoogleCalendar()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var flow = CreateGoogleAuthorizationCodeFlow();

        var request = flow.CreateAuthorizationCodeRequest(
            _configuration["GoogleAuth:RedirectUris:0"]
        );
        request.State = userId;

        var finalUri = request.Build();

        return Redirect(finalUri.AbsoluteUri);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        _logger.LogInformation("--- Google Callback Triggered ---");
        _logger.LogInformation("Received 'code' from Google: {Code}", code != null ? "Yes" : "No");
        _logger.LogInformation("Received 'state' (UserId) from Google: {State}", state);

        var userId = state;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("State (UserId) parameter is missing from Google callback.");
            return BadRequest("User identifier is missing from the callback.");
        }

        var flow = CreateGoogleAuthorizationCodeFlow();

        try
        {
            var token = await flow.ExchangeCodeForTokenAsync(
                userId, code, _configuration["GoogleAuth:RedirectUris:0"], CancellationToken.None);

            _logger.LogInformation("Successfully exchanged code for token. Refresh Token received: {HasToken}",
                !string.IsNullOrEmpty(token.RefreshToken) ? "Yes" : "No");

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                _logger.LogInformation("Found user {Email} in database to save the token.", user.Email);
                user.GoogleRefreshToken = token.RefreshToken;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully saved Refresh Token for user {Email}.", user.Email);
                }
                else
                {
                    _logger.LogError("Failed to save Refresh Token for user {Email}. Errors: {Errors}",
                        user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogError("Could not find user with ID {UserId} received from state.", userId);
            }

            return Ok("Google Calendar successfully connected! You can now close this tab.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred during token exchange in GoogleCallback.");
            return StatusCode(500, "An internal error occurred.");
        }
    }


    private GoogleAuthorizationCodeFlow CreateGoogleAuthorizationCodeFlow()
    {
        return new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _configuration["GoogleAuth:ClientId"],
                ClientSecret = _configuration["GoogleAuth:ClientSecret"]
            },
            Scopes = new[] { CalendarService.Scope.CalendarEvents } 
        });
    }
}