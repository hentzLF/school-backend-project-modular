using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using AgriMarket.Api.Hubs;
using AgriMarket.Modules.Bookings;
using AgriMarket.Modules.Marketplace;
using AgriMarket.Modules.Messaging;
using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Hubs;
using AgriMarket.Modules.Users;
using AgriMarket.Shared.Modules;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 7.4 — guard: fail fast if JWT signing key is absent
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is missing from configuration.");

// ---- Modules: each owns its services, DbContext, controllers and DB lifecycle ----
IModule[] modules =
[
    new UsersModule(),
    new MarketplaceModule(),
    new BookingsModule(),
    new MessagingModule(),
];

foreach (var module in modules)
    module.RegisterServices(builder.Services, builder.Configuration);

var moduleAssemblies = modules.Select(m => m.GetType().Assembly).ToArray();

// MediatR — integration events and their handlers span every module assembly
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(moduleAssemblies));

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Controllers live inside the module assemblies — register each as an MVC
// application part and discover their (internal) controllers via a custom
// feature provider.
var mvcBuilder = builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

foreach (var assembly in moduleAssemblies)
    mvcBuilder.AddApplicationPart(assembly);

mvcBuilder.ConfigureApplicationPartManager(manager =>
    manager.FeatureProviders.Add(new InternalControllerFeatureProvider()));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p
        .WithOrigins(
            builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? ["http://localhost:5173"])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

// 7.1 — JWT bearer authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

// 7.3
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("role", "Admin"));
});

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

// SignalR-backed real-time delivery for the Messaging module.
builder.Services.AddScoped<IMessageNotifier, SignalRMessageNotifier>();

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
    });
    c.AddSecurityRequirement(_ =>
    {
        var req = new OpenApiSecurityRequirement();
        req.Add(new OpenApiSecuritySchemeReference("Bearer"), new List<string>());
        return req;
    });
});

var app = builder.Build();

// Each module applies its own migrations and runs its seeders.
using (var scope = app.Services.CreateScope())
{
    foreach (var module in modules)
        await module.InitializeDatabaseAsync(scope.ServiceProvider);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/hubs/messages");

foreach (var module in modules)
    module.MapEndpoints(app);

app.Run();

/// <summary>
/// Discovers controllers regardless of visibility. Module controllers are
/// <c>internal</c> (the module core exposes no public surface beyond its
/// <c>IModule</c>), so the default public-only discovery would miss them.
/// </summary>
internal sealed class InternalControllerFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!typeInfo.IsClass || typeInfo.IsAbstract || typeInfo.ContainsGenericParameters)
            return false;

        if (typeInfo.IsDefined(typeof(NonControllerAttribute)))
            return false;

        return typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
            || typeInfo.IsDefined(typeof(ControllerAttribute));
    }
}
