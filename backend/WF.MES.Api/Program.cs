using FluentValidation;
using Serilog;
using System.Text.Json;
using WF.MES.Api.Filters;
using WF.MES.Api.Hubs;
using WF.MES.Api.Middleware;
using WF.MES.Api.Services;
using WF.MES.Application.Equipment;
using WF.MES.Application.Validators;
using WF.MES.Infrastructure;
using WF.MES.Infrastructure.Filters;
using WF.MES.Infrastructure.Messaging.Mqtt;
using WF.MES.Infrastructure.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/wf-mes-.log", rollingInterval: RollingInterval.Day);
});

builder.Services.AddControllers(options =>
    {
        options.Filters.Add<OperationLogFilter>();
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "WF.MES API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddSingleton<IDeviceTelemetryNotifier, DeviceTelemetryNotifier>();

var redisOptions = builder.Configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>()
    ?? new RedisOptions();
var sqlConnection = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' is missing.");

builder.Services.AddHealthChecks()
    .AddSqlServer(sqlConnection, name: "sqlserver")
    .AddRedis(redisOptions.ConnectionString, name: "redis")
    .AddCheck<MqttHealthCheck>("mqtt");

builder.Services.AddCors(options =>
{
    options.AddPolicy("WfCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddWfInfrastructure(builder.Configuration);
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LocalizationMiddleware>();
app.UseCors("WfCors");
app.UseAuthentication();
app.UseMiddleware<FactoryContextMiddleware>();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NoticeHub>("/hubs/notice");
app.MapHealthChecks("/health");

app.Run();
