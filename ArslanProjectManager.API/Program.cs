using ArslanProjectManager.Repository;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProjectManagerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"),
        option =>
            option.MigrationsAssembly(Assembly.GetAssembly(typeof(ProjectManagerDbContext))!.GetName().Name))
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
