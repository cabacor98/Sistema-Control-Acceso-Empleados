using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Domain.Repositories;

/// <summary>Puerto de persistencia del agregado Empleado.</summary>
public interface IEmpleadoRepository
{
    Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, Guid? idExcluido = null, CancellationToken cancellationToken = default);
    Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken = default);
}
