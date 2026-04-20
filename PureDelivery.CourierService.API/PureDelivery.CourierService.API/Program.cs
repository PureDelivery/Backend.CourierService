using MassTransit;
using Microsoft.EntityFrameworkCore;
using PureDelivery.Common.Configuration.Extensions;
using PureDelivery.Common.Configuration.Services;
using PureDelivery.Common.Http.Extensions;
using PureDelivery.Shared.Contracts.Configuration;
using PureDelivery.CourierService.API.Consumers;
using PureDelivery.CourierService.Core.Repositories;
using PureDelivery.CourierService.Core.Services;
using PureDelivery.CourierService.Core.Services.External;
using PureDelivery.CourierService.Core.Services.External.impl;
using PureDelivery.Shared.Contracts.Configuration;
using PureDelivery.CourierService.Core.Services.impl;
using PureDelivery.CourierService.Infrastructure.Data;
using PureDelivery.CourierService.Infrastructure.Repositories;
using Serilog;
using CourierServiceImpl = PureDelivery.CourierService.Core.Services.impl.CourierService;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("Service", "CourierService");
});

builder.Services.AddConfigurationProvider(builder.Configuration);

builder.Services.AddApiClient("HttpClient");

builder.Services.AddDbContext<CourierDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<ICourierRatingRepository, CourierRatingRepository>();
builder.Services.AddScoped<IAvailableOrderRepository, AvailableOrderRepository>();
builder.Services.AddScoped<ICourierAssignmentRepository, CourierAssignmentRepository>();
builder.Services.AddScoped<IIdentityServiceClient, IdentityServiceClient>();
builder.Services.AddScoped<ILocationServiceClient, LocationServiceClient>();
builder.Services.AddScoped<ICourierService, CourierServiceImpl>();
builder.Services.AddScoped<ICourierRatingService, CourierRatingService>();
builder.Services.AddScoped<ICourierAssignmentService, CourierAssignmentService>();

builder.Services.AddSingleton<RabbitMqConfiguration>(sp =>
{
    var provider = sp.GetRequiredService<ICustomConfigurationProvider>();
    var cfg = provider.GetConfigurationAsync<RabbitMqConfiguration>("RabbitMQ").Result;
    cfg.Validate();
    return cfg;
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderProcessedConsumer>();
    x.AddConsumer<OrderReadyForPickupConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitCfg = context.GetRequiredService<RabbitMqConfiguration>();
        cfg.Host(rabbitCfg.Host, rabbitCfg.VirtualHost, h =>
        {
            h.Username(rabbitCfg.Username);
            h.Password(rabbitCfg.Password);
        });
        cfg.ConfigureEndpoints(context);
    });
});

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
