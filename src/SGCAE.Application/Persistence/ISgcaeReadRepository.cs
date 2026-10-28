using SGCAE.Domain.Entities;

namespace SGCAE.Persistence.Abstractions;

/// <summary>
/// Contexto de solo lectura para consultas CQRS: expone proyecciones de
/// agregados sin métodos de modificación (separación comando/consulta).
/// </summary>
public interface ISgcaeReadRepository
{
    IQueryable<Empleado> Empleados { get; }
    IQueryable<Credencial> Credenciales { get; }
    IQueryable<PuntoAcceso> PuntosAcceso { get; }
    IQueryable<RegistroAcceso> RegistrosAcceso { get; }
}
