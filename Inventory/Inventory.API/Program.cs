using Inventory.Application;
using Inventory.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication();

// Infrastructure (DB)
builder.Services.AddInfrastructure(builder.Configuration);

// Add services to the container.
builder.Services.AddCors(o => o.AddPolicy("AllowAngularDev", p =>
{
    p.AllowAnyHeader()
     .AllowAnyMethod()
     //.WithOrigins("http://localhost:4200")
     .AllowAnyOrigin()// or the origin of your frontend
                      //.AllowCredentials()
     .WithExposedHeaders("Content-Disposition"); // <-- important
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseCors("AllowAngularDev");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
