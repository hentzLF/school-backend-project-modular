using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Testcontainers.PostgreSql;

namespace AgriMarket.E2E.Infrastructure;

public sealed class E2EFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private KestrelWebApplicationFactory _factory = null!;
    private IBrowser _browser = null!;
    private IPlaywright _playwright = null!;

    public string BaseUrl { get; private set; } = null!;

    public async Task<IBrowserContext> CreateBrowserContextAsync()
    {
        return await _browser.NewContextAsync();
    }

    public async Task<IPage> CreatePageAsync()
    {
        var context = await CreateBrowserContextAsync();
        return await context.NewPageAsync();
    }

    public async Task<IPage> CreateAuthenticatedClientPageAsync(
        string email, string password)
    {
        return await AuthHelper.LoginAsClientAsync(this, email, password);
    }

    public async Task<IPage> CreateAuthenticatedAdminPageAsync(
        string email, string password)
    {
        return await AuthHelper.LoginAsAdminAsync(this, email, password);
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            _postgres.GetConnectionString());

        _factory = new KestrelWebApplicationFactory();

        // Trigger host creation via CreateClient (which goes through EnsureServer -> CreateHost)
        _ = _factory.CreateClient();

        BaseUrl = _factory.ServerAddress;

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();
        if (_factory.RealHost is not null)
            await _factory.RealHost.StopAsync();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
    }

    private sealed class KestrelWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string ServerAddress { get; private set; } = "";
        public IHost? RealHost { get; private set; }
        public IServiceProvider RealServices => RealHost!.Services;

        public KestrelWebApplicationFactory()
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Build the real Kestrel host with full app configuration
            builder.ConfigureWebHost(webHostBuilder =>
            {
                webHostBuilder.UseKestrel();
                webHostBuilder.UseUrls("http://127.0.0.1:0");
            });

            RealHost = builder.Build();
            RealHost.Start();

            var server = RealHost.Services.GetRequiredService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();
            ServerAddress = addressFeature!.Addresses.First();

            // Return a dummy TestServer host to satisfy WebApplicationFactory's
            // internal cast of IServer -> TestServer
            var dummyBuilder = new HostBuilder();
            dummyBuilder.ConfigureWebHost(wb =>
            {
                wb.UseTestServer();
                wb.Configure(app => { });
            });
            var dummyHost = dummyBuilder.Build();
            dummyHost.Start();

            return dummyHost;
        }
    }
}
