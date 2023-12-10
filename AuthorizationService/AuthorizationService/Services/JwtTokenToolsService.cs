using AuthorizationApi.Database;
using AuthorizationApi.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthorizationService.Services;

public class TokensLifeTimeSettings
{
    public required TimeSpan LoginTokenDuration { get; set; }
    public required TimeSpan EmailValidationTokenDuration { get; set; }
    public required TimeSpan AccessCodeDuration { get; set; }
}

public class JwtTokenToolsSettings
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Key { get; set; }

    public void ConfigurateJwtBearerOptions(JwtBearerOptions jwtBearerOptions)
    {
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            ValidateIssuerSigningKey = true,
        };
    }
}

public interface IJwtTokenToolsService
{
    public string GenerateToken(string actor, TimeSpan duration);
    public void SetLoginJwtTokenHeader(User user, IHeaderDictionary header);
    public EmailVerification CreateEmailVerification(User user);
    public bool ValidateToken(string token, out User? user);
}

public class JwtTokenToolsService : IJwtTokenToolsService
{
    private readonly JwtTokenToolsSettings tokenSettings;
    private readonly TokensLifeTimeSettings tokensLifeTimeSettings;
    private readonly AuthorizationDbContext authorizationDbContext;
    private readonly ILogger<JwtTokenToolsService> logger;

    public JwtTokenToolsService(
        IOptions<JwtTokenToolsSettings> tokenSettings,
        IOptions<TokensLifeTimeSettings> tokensLifeTimeSettings,
        AuthorizationDbContext dbContext,
        ILogger<JwtTokenToolsService> logger
    )
    {
        this.logger = logger;
        this.tokenSettings = tokenSettings.Value;
        this.tokensLifeTimeSettings = tokensLifeTimeSettings.Value;
        authorizationDbContext = dbContext;
    }

    public EmailVerification CreateEmailVerification(User user)
    {
        return new()
        {
            User = user,
            JwtToken = GenerateToken(
                user.Login,
                tokensLifeTimeSettings.EmailValidationTokenDuration
            ),
            RequestDate = DateTime.UtcNow
        };
    }

    public string GenerateToken(string actor, TimeSpan duration)
    {
        List<Claim> claims = new() { new Claim(ClaimTypes.Actor, actor) };
        JwtSecurityToken token =
            new(
                issuer: tokenSettings.Issuer,
                audience: tokenSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(duration),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Key)),
                    SecurityAlgorithms.HmacSha256
                )
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public void SetLoginJwtTokenHeader(User user, IHeaderDictionary header)
    {
        header.Add(
            "JwtBearerToken",
            GenerateToken(user.Login, tokensLifeTimeSettings.LoginTokenDuration)
        );
    }

    public bool ValidateToken(string token, out User? user)
    {
        if (token == null)
        {
            logger.LogInformation("Null token");
            user = null;
            return false;
        }

        JwtSecurityTokenHandler tokenHandler = new();
        try
        {
            ClaimsPrincipal calms = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(tokenSettings.Key)
                    ),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                },
                out _
            );
            string userLogin = calms.Claims.First(x => x.Type == ClaimTypes.Actor).Value;
            user = authorizationDbContext.Users.ToList().Find(user => user.Login == userLogin);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogInformation("Exception in validating: " + ex);
            user = null;
            return false;
        }
    }
}
