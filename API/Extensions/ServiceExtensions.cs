using Microsoft.EntityFrameworkCore;
using Repositories.Implementation;
using Repositories.Interface;
using Microsoft.OpenApi.Models;
using BusinessObjects.Entities;
using Services.Interface;
using Services.Implementation;
using FluentValidation;
using Validations.Category;
using Validations.Member;
using Validations.Order;
using Validations.Product;
using Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BusinessObjects.Dto.Auth;
using System.Security.Claims;
using Services.Client.Cache;
using Validations.OrderDetail;

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
            // Authetication Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPasswordService, BCryptPasswordService>();

            // Bussiness Services
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();
            return services;
        }

        public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<eStoreDBContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DBConnection")));
            return services;
        }

        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("RedisConnection");
                options.InstanceName = "SampleInstance";
            });
            services.AddScoped<ICacheService, RedisCacheService>();
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

                // Add JWT authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization. Enter your token only, 'Bearer' prefix will be added automatically",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
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
            
            #region OrderDetail
            services.AddValidatorsFromAssemblyContaining<OrderDetailForCreationValidator>();
            services.AddValidatorsFromAssemblyContaining<OrderDetailForUpdateValidator>();
            #endregion
            

            return services;
        }

        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind JWT settings from configuration
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // Add JWT authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Custom Token Revocation Check
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var redisCacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
                        var rawToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

                        // Access claims directly from the ClaimsPrincipal
                        var userIdClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier);
                        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                        {
                            context.Fail("Invalid user ID claim.");
                            return;
                        }

                        if (!(await redisCacheService.IsInWhitelist(userId, rawToken)))
                        {
                            context.Fail("Token has been revoked.");
                        }
                    }
                };
            });

            services.AddScoped<JwtProvider>();

            return services;
        }
    }
}