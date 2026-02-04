using ArslanProjectManager.API.Filters;
using ArslanProjectManager.API.Middlewares;
using ArslanProjectManager.API.Modules;
using ArslanProjectManager.Repository;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Service.Mappings;
using ArslanProjectManager.Service.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecurityKey"]!)),
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = ClaimTypes.Role
        };

        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader["Bearer ".Length..].Trim();
                }
                else if (context.Request.Cookies.ContainsKey("AccessToken"))
                {
                context.Token = context.Request.Cookies["AccessToken"];
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                if (context.AuthenticateFailure != null)
                {
                    context.Response.Headers.Append("Authentication-Error",
                        context.AuthenticateFailure.Message.Replace("\n", " ").Replace("\r", " "));
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProjectManagerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"),
        option =>
            option.MigrationsAssembly(Assembly.GetAssembly(typeof(ProjectManagerDbContext))!.GetName().Name))
    );

builder.Services.AddScoped(typeof(NotFoundFilter<>));
builder.Services.AddAutoMapper(cfg => cfg.AllowNullCollections = true, typeof(MapProfile));

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new RepositoryServiceModule());
});

var app = builder.Build();

// Seed initial static data (idempotent: only inserts when tables are empty)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeederService>();
    await seeder.SeedAsync();
}
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
        options.RoutePrefix = "api-docs";

    });
    app.UseReDoc(options =>
    {
        options.SpecUrl = "/openapi/v1.json";
        options.RoutePrefix = "api-docs/redoc";
    });

    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCustomException();

app.UseMiddleware<TokenExpirationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/", () => Results.Redirect("/api-docs", permanent: true)).ExcludeFromDescription();
app.MapControllers();

app.Run();
