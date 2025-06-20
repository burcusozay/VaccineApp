using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VaccineApp.Business.AutoMapper;
using VaccineApp.Business.Base;
using VaccineApp.Business.Register;
using VaccineApp.Business.Repository;
using VaccineApp.Business.UnitOfWork;
using VaccineApp.Data.Context;
using VaccineApp.ViewModel.Options;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

        // Redis (IDistributedCache icin StackExchange Redis kullanimi)
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["Redis:ConnectionString"];
            options.InstanceName = "VaccineCache:";
        });

        builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<AuditActionFilter>();
        builder.Services.AddHttpContextAccessor(); // Username alabilmek icin
        builder.Services.AddAutoMapper(typeof(AutoMappingProfile));

        builder.Services.UseRegister();

        var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(",") ?? new[] { "*" };
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Add services to the container.
        builder.Services.AddControllers(options =>
        {
            //options.Filters.Add<UnitOfWorkTransactionFilter>();
            options.Filters.AddService<AuditActionFilter>();
        });

        // JWT konfigürasyonu (appsettings.json -> "Jwt" section kullanýlmalý)
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtConfig["SecretKey"]);
        builder.Services.Configure<JWTSettingOptions>(jwtConfig);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,
               ValidIssuer = jwtConfig["Issuer"],
               ValidAudience = jwtConfig["Audience"],
               IssuerSigningKey = new SymmetricSecurityKey(key)
           };
       });


        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            // Swagger JWT Auth desteklemesi
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement{{
                new OpenApiSecurityScheme{ Reference = new OpenApiReference{ Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                                            Scheme = "oauth2",
                                            Name = "Bearer",
                                            In = ParameterLocation.Header
                    },
                Array.Empty<string>()    }});
        });
        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            //app.MapOpenApi();
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vaccine API"); });
        }
        app.UseHttpsRedirection();
        app.UseMiddleware<TransactionMiddleware>();
        app.UseCors();
        app.UseAuthentication(); // Auth middleware aktif edilmeli
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}