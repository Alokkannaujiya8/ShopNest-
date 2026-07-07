using System;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using ShopNest.API.Middleware;
using ShopNest.Application;
using ShopNest.Infrastructure;
using ShopNest.Infrastructure.Hubs;
using ShopNest.Infrastructure.Persistence;
using ShopNest.Infrastructure.Settings;

// Bootstrap Serilog
Log.Logger = new LoggerConfiguration()


    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/shopnest-log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: 
        "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [CorrelationId: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting ShopNest Web API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Register controllers & serialization settings
    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddSignalR();

    // CORS configurations
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader()
        );
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();

    // Swagger configurations (Multi-version support)
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "ShopNest API v1", Version = "v1" });
        options.SwaggerDoc("v2", new OpenApiInfo { Title = "ShopNest API v2", Version = "v2" });
        
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        });
        
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
        });
    });

    // JWT Authentication
    var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Query["access_token"];
                    if (!string.IsNullOrWhiteSpace(token) && context.HttpContext.Request.Path.StartsWithSegments("/hubs/orders"))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.PermitLimit = 100;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueLimit = 0;
        });
    });

    // Response Compression
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
    });

    // Health Checks
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    var redisString = builder.Configuration.GetConnectionString("Redis");
    var healthChecks = builder.Services.AddHealthChecks();
    if (!string.IsNullOrWhiteSpace(connString))
    {
        healthChecks.AddCheck("SQL Server", () =>
        {
            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(connString);
                conn.Open();
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("SQL Server connection failed.", ex);
            }
        });
    }
    if (!string.IsNullOrWhiteSpace(redisString))
    {
        healthChecks.AddCheck("Redis Cache", () =>
        {
            try
            {
                var options = StackExchange.Redis.ConfigurationOptions.Parse(redisString);
                options.ConnectTimeout = 1000;
                using var conn = StackExchange.Redis.ConnectionMultiplexer.Connect(options);
                conn.GetDatabase().Ping();
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Redis ping failed.", ex);
            }
        });
    }

    var app = builder.Build();

    // Database migration & seeding
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ShopNestDbContext>();
        try
        {
            await context.Database.MigrateAsync();
            await DbSeeder.SeedAsync(context);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred during database migration or seeding.");
        }
    }

    // Middleware Pipeline Order
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();

    app.UseResponseCompression();
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShopNest API v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "ShopNest API v2");
        });
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("Frontend");

    var supportedCultures = new[] { "en", "hi" };
    var localizationOptions = new RequestLocalizationOptions()
        .SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
    app.UseRequestLocalization(localizationOptions);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<OrderHub>("/hubs/orders");
    app.MapHangfireDashboard("/hangfire").RequireAuthorization();
    app.MapHealthChecks("/health");

    // Register Background Cleanup Jobs
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            RecurringJob.AddOrUpdate<ShopNestDbContext>("CleanupExpiredOtps", db => db.CleanupExpiredOtps(), Cron.Hourly);
            RecurringJob.AddOrUpdate<ShopNestDbContext>("CleanupRevokedTokens", db => db.CleanupRevokedTokens(), Cron.Daily);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize Hangfire cleanup jobs.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
