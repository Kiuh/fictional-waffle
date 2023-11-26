using AuthorizationApi.Database;
using AuthorizationService.Controllers;
using AuthorizationService.Middleware;
using AuthorizationService.Services;
using AuthorizationService.Services.Mail;
using AuthorizationService.Services.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

string? envVar = Environment.GetEnvironmentVariable("ROOM_MANAGER_IP_ADDRESS");
string roomManagerApiPath = envVar != null ? $"http://{envVar}" : "http://localhost:5132";

envVar = Environment.GetEnvironmentVariable("STATISTIC_SERVICE_IP_ADDRESS");
string statisticServiceApiPath = envVar != null ? $"http://{envVar}" : "http://localhost:5132";

envVar = Environment.GetEnvironmentVariable("MAIL_VERIFICATION_IP_ADDRESS");
string mailVerificationLink =
    envVar != null ? $"https://{envVar}/Verification" : "https://localhost:5000/Verification";

string databasePort = Environment.GetEnvironmentVariable("DATABASE_PORT") ?? "5432";
string databaseHost = Environment.GetEnvironmentVariable("DATABASE_HOST") ?? "localhost";
string databaseConnectionString =
    $"Host={databaseHost};Port={databasePort};Database=LifeCreatorDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AuthorizationDbContext>(
    options => options.UseNpgsql(databaseConnectionString)
);

builder.Services.AddTransient<IUsersService, UsersService>();
builder.Services.AddTransient<IEmailVerificationsService, EmailVerificationsService>();
builder.Services.AddTransient<IPasswordRecoversService, PasswordRecoversService>();

builder.Services.Configure<MailSenderSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailSenderService, MailSenderService>();

builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo() { Title = "Auth API", Version = "v1" });
});
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
    (settings) => settings.VerificationLink = mailVerificationLink
);

builder.Services.Configure<RedirectionSettings>(
    (settings) =>
    {
        settings.RoomManagerApiPath = roomManagerApiPath;
        settings.StatisticServiceApiPath = statisticServiceApiPath;
    }
);

builder.Services.AddTransient<IMailBodyBuilder, MailBodyBuilderService>();

builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

WebApplication app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

_ = app.UseSwagger();
_ = app.UseSwaggerUI();

_ = app.UseAuthentication();
_ = app.UseAuthorization();

_ = app.MapControllers();
_ = app.MapRazorPages();

IDbInitializeService initService = app.Services.GetRequiredService<IDbInitializeService>();

initService.InitializeDb();

app.Run();
