using eStore.Components;
using eStore.Services;

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

            // Configure HttpClient with auth handler
            builder.Services.AddScoped<AuthenticationHeaderHandler>();
            builder.Services.AddHttpClient("API", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7173/");
            })
            .AddHttpMessageHandler<AuthenticationHeaderHandler>();

            // Add services
            builder.Services.AddScoped<LocalStorageService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<StateContainer>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
