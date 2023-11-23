using Microsoft.EntityFrameworkCore;
using StatisticServiceApi.DataBase;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string connectionString;

if (builder.Environment.EnvironmentName is "DockerDevelopment" or "Production")
{
    connectionString =
        "Host=statistic_postgres_db;Port=5434;Database=statistic_db;Username=postgres;Password=postgres";
}
else
{
    connectionString =
        "Host=localhost;Port=5434;Database=statistic_db;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<StatisticDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
