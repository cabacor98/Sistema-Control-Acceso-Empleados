using SGCAE.Application;
using SGCAE.Application.Queries;
using SGCAE.Infrastructure;
using SGCAE.Infrastructure.Persistence;
using SGCAE.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SGCAE.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ---------- Capas ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("SgcaeDb")!);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------- Inicialización ----------
await InicializadorBaseDatos.EjecutarAsync(app.Services);

app.UseMiddleware<ManejadorExcepcionesMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // /scalar
}

app.UseHttpsRedirection();
app.MapControllers();

// ---------- Raíz informativa ----------
app.MapGet("/", () => Results.Ok(new
{
    nombre = "SGCAE — Sistema de Gestión y Control de Acceso de Empleados",
    documentacion = new { openApi = "/openapi/v1.json", interfaz = "/scalar" },
    recursos = new[] { "/api/empleados", "/api/credenciales", "/api/accesos", "/api/puntos-acceso" }
}));

app.Run();
