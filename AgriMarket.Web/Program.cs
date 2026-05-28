using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using AgriMarket.Modules.Bookings;
using AgriMarket.Modules.Marketplace;
using AgriMarket.Modules.Messaging;
using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Users;
using AgriMarket.Resources;
using AgriMarket.Shared.Modules;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

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
.AddMvc();

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .ConfigureApplicationPartManager(manager =>
        manager.FeatureProviders.Add(new InternalControllerFeatureProvider()));
builder.Services.AddLocalization();
builder.Services.AddScoped<IMessageNotifier, AgriMarket.Web.Services.NoOpMessageNotifier>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("et") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Client/Account/Login";
        options.AccessDeniedPath = "/Client/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                var redirect = ctx.Request.Path.StartsWithSegments("/Admin")
                    ? "/Admin/Account/Login"
                    : "/Client/Account/Login";
                ctx.Response.Redirect(redirect + ctx.Request.QueryString);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                var redirect = ctx.Request.Path.StartsWithSegments("/Admin")
                    ? "/Admin/Account/AccessDenied"
                    : "/Client/Account/AccessDenied";
                ctx.Response.Redirect(redirect);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Admin"));

    options.AddPolicy("ClientOnly", policy =>
        policy.RequireClaim(ClaimTypes.Role, "Client"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    foreach (var module in modules)
        await module.InitializeDatabaseAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseRouting();
app.UseRequestLocalization();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "client",
    areaName: "Client",
    pattern: "Client/{controller=Listings}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

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
