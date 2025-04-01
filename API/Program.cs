using API.Extensions;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
        });

        builder.Services
            .AddDbContext(builder.Configuration)
            .AddRepositories()
            .AddBusinessServices()
            .AddCorsPolicy()
            .AddAutoMapper(typeof(MappingProfile))
            .AddSwagger()
            .AddJwtAuthentication(builder.Configuration)
            .AddRedisCache(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAuthorization();
        builder.Services.ConfigureFluentValidations();

        builder.Services.AddDbContext<eStoreDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection"))
                .EnableSensitiveDataLogging());

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseSerilogRequestLogging();
        app.UseRouting();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
            c.InjectStylesheet("/swagger-ui/SwaggerDark.css");
        });
         app.UseHttpsRedirection();
        app.UseCors("AllowFrontendApp");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        try
        {
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application encountered a fatal error!");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
