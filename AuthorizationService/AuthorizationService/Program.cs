using AuthorizationApi.Database;
using AuthorizationService.Controllers;
using AuthorizationService.Middleware;
using AuthorizationService.Services;
using AuthorizationService.Services.Mail;
using AuthorizationService.Services.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

string connectionStringSection;
string mailBodyBuilderSettingsSection;
string redirectionSettingsSection;

string databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT");
string databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST");
string databaseConnectionString = $"Host={databaseHost};Port={databasePort};Database=LifeCreatorDb;Username=postgres;Password=postgres";

if (builder.Environment.EnvironmentName is "DockerDevelopment" or "Production")
{
    connectionStringSection = "AuthorizationDbContextDocker";
    mailBodyBuilderSettingsSection = "MailBodyBuilderSettingsDocker";
    redirectionSettingsSection = "RedirectionSettingsDocker";
}
else
{
    if (builder.Environment.EnvironmentName is not "DesktopDevelopment")
    {
        throw new Exception("Unknown Environment");
    }
    else
    {
        connectionStringSection = "AuthorizationDbContextWindows";
        mailBodyBuilderSettingsSection = "MailBodyBuilderSettingsDesktop";
        redirectionSettingsSection = "RedirectionSettingsDesktop";
    }
}

builder.Services.AddDbContext<AuthorizationDbContext>(
    options => options.UseNpgsql(databaseConnectionString)
);

builder.Services.AddTransient<IUsersService, UsersService>();
builder.Services.AddTransient<IEmailVerificationsService, EmailVerificationsService>();
builder.Services.AddTransient<IPasswordRecoversService, PasswordRecoversService>();

builder.Services.Configure<MailSenderSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailSenderService, MailSenderService>();

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.Configure<TokensLifeTimeSettings>(
    builder.Configuration.GetSection("TokensLifeTimeSettings")
);

IConfigurationSection jwtTokenServiceSettingsConfig = builder.Configuration.GetSection(
    "JwtTokenServiceSettings"
);

builder.Services.Configure<JwtTokenToolsSettings>(jwtTokenServiceSettingsConfig);

JwtTokenToolsSettings? jwtTokenServiceSettings =
    jwtTokenServiceSettingsConfig.Get<JwtTokenToolsSettings>()
    ?? throw new Exception("No JwtTokenServiceSettings");

builder.Services.AddTransient<IJwtTokenToolsService, JwtTokenToolsService>();

builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtTokenServiceSettings.ConfigurateJwtBearerOptions);

builder.Services.AddSingleton<IDbInitializeService, DbInitializeService>();

builder.Services.Configure<CryptographyServiceSettings>(
    builder.Configuration.GetSection("CryptographySettings")
);

builder.Services.AddTransient<ICryptographyService, CryptographyService>();

builder.Services.Configure<MailBodyBuilderSettings>(
    builder.Configuration.GetSection(mailBodyBuilderSettingsSection)
);
builder.Services.Configure<RedirectionSettings>(
    builder.Configuration.GetSection(redirectionSettingsSection)
);
builder.Services.AddTransient<IMailBodyBuilder, MailBodyBuilderService>();

builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

WebApplication app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.EnvironmentName is "DesktopDevelopment")
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

_ = app.UseAuthentication();
_ = app.UseAuthorization();

_ = app.MapControllers();
_ = app.MapRazorPages();

IDbInitializeService initService = app.Services.GetRequiredService<IDbInitializeService>();
if (app.Environment.EnvironmentName is "DockerDevelopment" or "DesktopDevelopment")
{
    initService.InitializeDb();
}
else if (app.Environment.EnvironmentName is "Production")
{
    initService.MigrateDb();
}

app.Run();
