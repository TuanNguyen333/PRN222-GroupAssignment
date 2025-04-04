using eStore.Components;
using Microsoft.EntityFrameworkCore;
using eStore.Services;
using eStore.Hubs;
using eStore.Services.Auth;
using eStore.Services.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace eStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add JWT authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });

            // Configure HttpClient for API calls with common settings
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true,
                UseProxy = false,
                AllowAutoRedirect = true
            };

            // Register services
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IMemberService, MemberService>();
            builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();

            // Configure HttpClient for ProductService
            builder.Services.AddHttpClient<IProductService, ProductService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            }).ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);

            // Configure HttpClient for CategoryService
            builder.Services.AddHttpClient<ICategoryService, CategoryService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            }).ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);

            // Configure HttpClient for MemberService
            builder.Services.AddHttpClient<IMemberService, MemberService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<AuthenticationHeaderHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);

            // Configure HttpClient for OrderDetailService
            builder.Services.AddHttpClient<IOrderDetailService, OrderDetailService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            }).ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);

            // Add SignalR for real-time updates
            builder.Services.AddSignalR();

            // Add core services first
            builder.Services.AddSingleton<StateContainer>();
            builder.Services.AddScoped<AuthService>();

            // Configure HttpClient with auth handler
            builder.Services.AddScoped<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<AuthenticationHeaderHandler>();

            // Register other services that depend on HttpClient
            builder.Services.AddScoped<IOrderService, OrderService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseMigrationsEndPoint();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            // Add authentication middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Map SignalR hubs for real-time updates
            app.MapHub<ProductHub>("/producthub");
            app.MapHub<OrderHub>("/orderhub");
            app.MapHub<MemberHub>("/memberhub");
            app.MapHub<CategoryHub>("/categoryhub");  // CategoryHub is already registered here

            app.Run();
        }
    }
}
