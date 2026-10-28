using SGCAE.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SGCAE.Persistence.Abstractions;
using SGCAE.Infrastructure.Persistence;
using SGCAE.Infrastructure.Persistence.Repositories;

namespace SGCAE.Infrastructure;

/// <summary>
/// Registro de la implementación de infraestructura: DbContext de SQL Server,
/// repositorios, unidad de trabajo y acceso de lectura para consultas CQRS.
/// </summary>
public static class InyeccionDependencias
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string cadenaConexion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);

        services.AddDbContext<SgcaeDbContext>(options =>
        {
            options.UseSqlServer(cadenaConexion, sql =>
            {
                sql.MigrationsAssembly(typeof(InyeccionDependencias).Assembly);
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            });
        });

        services.AddScoped<ISgcaeReadRepository, SgcaeReadRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<ICredencialRepository, CredencialRepository>();
        services.AddScoped<IPuntoAccesoRepository, PuntoAccesoRepository>();
        services.AddScoped<IRegistroAccesoRepository, RegistroAccesoRepository>();

        return services;
    }
}
