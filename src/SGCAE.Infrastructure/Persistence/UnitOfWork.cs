using SGCAE.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using SGCAE.Persistence.Abstractions;

namespace SGCAE.Infrastructure.Persistence;

/// <summary>
/// Implementación de la unidad de trabajo sobre EF Core: todas las escrituras
/// realizadas por los repositorios se guardan en una única transacción.
/// </summary>
internal sealed class UnitOfWork(SgcaeDbContext context) : IUnitOfWork
{
    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}

/// <summary>
/// Contexto de solo lectura para las consultas CQRS (RF-04): expone
/// IQueryable sin exponer capacidades de modificación.
/// </summary>
internal sealed class SgcaeReadRepository(SgcaeDbContext context) : ISgcaeReadRepository
{
    public IQueryable<SGCAE.Domain.Entities.Empleado> Empleados => context.Empleados;
    public IQueryable<SGCAE.Domain.Entities.Credencial> Credenciales => context.Credenciales;
    public IQueryable<SGCAE.Domain.Entities.PuntoAcceso> PuntosAcceso => context.PuntosAcceso;
    public IQueryable<SGCAE.Domain.Entities.RegistroAcceso> RegistrosAcceso => context.RegistrosAcceso;
}
