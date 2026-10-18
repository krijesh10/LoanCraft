using LoanCraft.API.Middleware;
using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Application.Services;
using LoanCraft.Infrastructure;
using LoanCraft.Infrastructure.RealTime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Text;

// 1. Configure Serilog Bootstrap Logger
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/loancraft-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting LoanCraft API Host...");
    var builder = WebApplication.CreateBuilder(args);

    // Plug Serilog into ASP.NET Core host
    builder.Host.UseSerilog();

    // 2. Exception Handling & RFC 7807 ProblemDetails Services
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // 3. Application Services
    builder.Services.AddScoped<ILoanService, LoanService>();
    builder.Services.AddScoped<IUnderwritingService, UnderwritingService>();
    builder.Services.AddScoped<IAuthService, AuthService>();

    // 4. Infrastructure Services (EF Core, SignalR, HttpClient, PasswordHasher, Jwt)
    builder.Services.AddInfrastructure(builder.Configuration);

    // 5. Controllers & Swagger
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // 6. CORS for Angular dev
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularDev", policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .WithExposedHeaders("X-Correlation-ID"); // Expose correlation ID to Angular
        });
    });

    // 7. JWT Authentication & SignalR WebSocket Auth
    //var jwtSecret = builder.Configuration["JwtSettings:Secret"]!;
    //var key = Encoding.UTF8.GetBytes(jwtSecret);

    //builder.Services.AddAuthentication(options =>
    //{
    //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //})
    //.AddJwtBearer(options =>
    //{
    //    options.RequireHttpsMetadata = false;
    //    options.SaveToken = true;
    //    options.TokenValidationParameters = new TokenValidationParameters
    //    {
    //        ValidateIssuerSigningKey = true,
    //        IssuerSigningKey = new SymmetricSecurityKey(key),
    //        ValidateIssuer = true,
    //        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
    //        ValidateAudience = true,
    //        ValidAudience = builder.Configuration["JwtSettings:Audience"],
    //        ValidateLifetime = true,
    //        ClockSkew = TimeSpan.Zero
    //    };

    //    options.Events = new JwtBearerEvents
    //    {
    //        OnMessageReceived = context =>
    //        {
    //            var accessToken = context.Request.Query["access_token"];
    //            var path = context.HttpContext.Request.Path;
    //            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
    //            {
    //                context.Token = accessToken;
    //            }
    //            return Task.CompletedTask;
    //        }
    //    };
    //});


    // 7. OIDC JWT Authentication via Keycloak & SignalR WebSocket Auth
    var keycloakAuthority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/LoanCraft";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = false; // false for local HTTP development
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,
            ValidateAudience = false, // Keycloak access tokens target the realm/account
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "preferred_username", // Keycloak maps username to preferred_username
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            // CRITICAL ENTERPRISE FEATURE: Keycloak Role Mapping
            // Keycloak embeds realm roles inside JSON: "realm_access": { "roles": ["Underwriter"] }
            // This hook extracts them so [Authorize(Roles = "Underwriter")] works in C#!
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity claimsIdentity)
                {
                    var realmAccessClaim = claimsIdentity.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccessClaim))
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(realmAccessClaim);
                        if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                        {
                            foreach (var role in rolesElement.EnumerateArray())
                            {
                                claimsIdentity.AddClaim(new System.Security.Claims.Claim(
                                    System.Security.Claims.ClaimTypes.Role,
                                    role.GetString()!));
                            }
                        }
                    }
                }
                return Task.CompletedTask;
            },

            // SignalR WebSockets Token Support (passes access_token from query string)
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });




    // 8. Swagger with Bearer Lock
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "LoanCraft API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and your token.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", doc),
                new List<string>()
            }
        });
    });

    var app = builder.Build();

    // ==========================================
    // HTTP Pipeline Configuration (CRITICAL ORDER)
    // ==========================================
    app.UseMiddleware<CorrelationIdMiddleware>(); // 1. First: Generate/read X-Correlation-ID & enrich logs
    app.UseExceptionHandler();                   // 2. Second: Catch and format all exceptions to RFC 7807
    app.UseSerilogRequestLogging();              // 3. Third: Log HTTP method, path, status & duration

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAngularDev");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<WorkflowHub>("/hubs/workflow");

    // 9. Seed Test Underwriter Account
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        if (!db.Users.Any(u => u.Email == "underwriter@loancraft.com"))
        {
            db.Users.Add(new LoanCraft.Domain.Entities.User
            {
                FullName = "Lead Underwriter",
                Email = "underwriter@loancraft.com",
                PasswordHash = hasher.HashPassword("Password123!"),
                Role = "Underwriter"
            });
            db.SaveChangesAsync().GetAwaiter().GetResult();
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "LoanCraft API terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}
