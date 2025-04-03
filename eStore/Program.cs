using eStore.Components;
using Microsoft.EntityFrameworkCore;
using eStore.Services;
using eStore.Hubs;
using eStore.Services.Auth;
using eStore.Services.Common;

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
            builder.Services.AddScoped<IOrderService, OrderService>();
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


            // Configure HttpClient for OrderService
            builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
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
            }).ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);

            // Configure HttpClient for OrderDetailService
            builder.Services.AddHttpClient<IOrderDetailService, OrderDetailService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(30);
            }).ConfigurePrimaryHttpMessageHandler(() => httpClientHandler);

            builder.Services.AddSignalR();

            // Configure HttpClient with auth handler
            builder.Services.AddScoped<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
            })
            .AddHttpMessageHandler<AuthenticationHeaderHandler>();

            // Add services
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<LocalStorageService>();
            builder.Services.AddScoped<StateContainer>();

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

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapHub<ProductHub>("/producthub");
            app.MapHub<OrderHub>("/orderhub");
            app.MapHub<MemberHub>("/memberhub");

            app.Run();
        }
    }
}
