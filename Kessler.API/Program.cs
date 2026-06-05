using System.Text;
using System.Threading.RateLimiting;
using Kessler.API.Middleware;
using Kessler.Application.Interfaces;
using Kessler.Services;
using Kessler.Domain.Interfaces;
using Kessler.Infrastructure.Data;
using Kessler.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Limite de tamanho do corpo da requisição (previne payload flooding) ──────
var maxBodySize = builder.Configuration.GetValue<long>("RequestLimits:MaxBodySizeBytes", 1_048_576);
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = maxBodySize);

// ─── Banco de Dados ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<KesslerDbContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        oracle => oracle.MigrationsAssembly("Kessler.Infrastructure")
    )
);

// ─── Repositórios (Infrastructure) ───────────────────────────────────────────
builder.Services.AddScoped<IOrbitalObjectRepository, OrbitalObjectRepository>();
builder.Services.AddScoped<IMissionRepository, MissionRepository>();
builder.Services.AddScoped<IReuseMaterialRepository, ReuseMaterialRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ─── Serviços (Application) ───────────────────────────────────────────────────
builder.Services.AddScoped<IOrbitalObjectService, OrbitalObjectService>();
builder.Services.AddScoped<IMissionService, MissionService>();
builder.Services.AddScoped<IReuseMaterialService, ReuseMaterialService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não configurada em appsettings.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ─── Rate Limiting (proteção DDoS / brute force) ─────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 100,
                QueueLimit = 0
            }
        )
    );
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ─── CORS — origens autorizadas (nunca AllowAnyOrigin em produção) ─────────────
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedCors", policy =>
    {
        if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        else
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
    });
});

// ─── Controllers + validação de modelo automática ([ApiController]) ───────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Kessler API",
        Version = "v1",
        Description = "API para monitoramento e gerenciamento de detritos orbitais — Projeto Kessler OS"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ─── Pipeline de Middleware (ordem importa) ───────────────────────────────────
app.UseMiddleware<SecurityHeadersMiddleware>();   // 1. Headers de segurança
app.UseMiddleware<ExceptionMiddleware>();          // 2. Tratamento de erros
app.UseMiddleware<SanitizationMiddleware>();       // 3. Sanitização XSS/SQLi
app.UseMiddleware<PayloadIntegrityMiddleware>();   // 4. Verificação de integridade (HMAC)
app.UseMiddleware<AuditMiddleware>();              // 5. Trilha de auditoria

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("RestrictedCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ─── Migrações automáticas ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<KesslerDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        log.LogError(ex, "Erro ao aplicar migrações do banco de dados.");
    }
}

app.Run();
