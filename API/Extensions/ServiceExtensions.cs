using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Implementation;
using Repositories.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.OpenApi.Models;
using BusinessObjects.Entities;
using Services.Interface;
using Services.Implementation;
using FluentValidation;
using Validations;
using BusinessObjects.Dto.Category;
using Validations.Category;
using BusinessObjects.Dto.Member;
using BusinessObjects.Dto.Order;
using BusinessObjects.Dto.Product;
using Validations.Member;
using Validations.Order;
using Validations.Product;

namespace API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }

        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            return services;
        }

        public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<eStoreDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DBConnection")));
            return services;
        }

        public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendApp", policy =>
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials());
            });
            return services;
        }

        //public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        //{
        //    Log.Logger = new LoggerConfiguration()
        //        .ReadFrom.Configuration(configuration)
        //        .MinimumLevel.Information()
        //        .WriteTo.Console()
        //        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
        //        .CreateLogger();

        //    services.AddSerilog();
        //    return services;
        //}

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "API for managing categories, members, products, and orders"
                });
            });
            return services;
        }



        public static IServiceCollection ConfigureFluentValidations(this IServiceCollection services)
        {
            #region Category
            services.AddValidatorsFromAssemblyContaining<CategoryForCreationValidator>();
            services.AddValidatorsFromAssemblyContaining<CategoryForUpdateValidator>();
            #endregion

            #region Member
            services.AddValidatorsFromAssemblyContaining<MemberForCreationValidator>();
            services.AddValidatorsFromAssemblyContaining<MemberForUpdateValidator>();
            #endregion

            #region Order
            services.AddValidatorsFromAssemblyContaining<OrderForCreationValidator>();
            services.AddValidatorsFromAssemblyContaining<OrderForUpdateValidator>();
            #endregion

            #region Product
            services.AddValidatorsFromAssemblyContaining<ProductForCreationValidator>();
            services.AddValidatorsFromAssemblyContaining<ProductForUpdateValidator>();
            #endregion

            return services;
        }
    }
}