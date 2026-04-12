using PureDelivery.Common.Configuration.Extensions;
using PureDelivery.Common.Http.Extensions;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Core.Services;
using PureDelivery.CourierService.Core.Services.External;
using PureDelivery.CourierService.Core.Services.External.impl;
using PureDelivery.CourierService.Core.Services.impl;
using PureDelivery.CourierService.Helpers;
using PureDelivery.CourierService.Infrastructure.Repositories;
using PureDelivery.Infrastructure.Redis.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("Service", "CourierService");
});

builder.Services.AddRedisServices("Redis");
builder.Services.AddConfigurationProvider(builder.Configuration);

builder.Services.AddApiClient("HttpClient");

builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IIdentityServiceClient, IdentityServiceClient>();
builder.Services.AddScoped<ICourierService, CourierService>();

await IoCHelper.ConfigureDatabaseAsync(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "PureDelivery Courier Service API",
        Version = "v1",
        Description = "API для управления курьерами",
        Contact = new()
        {
            Name = "PureDelivery Team",
            Email = "support@puredelivery.com"
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Courier Service API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
