using SGCAE.Domain.Entities;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Domain.Repositories;

/// <summary>Puerto de persistencia de credenciales. Opera directamente sobre el agregado Credencial.</summary>
public interface ICredencialRepository
{
    Task<Credencial?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Credencial?> ObtenerPorCodigoAsync(CodigoCredencial codigo, CancellationToken cancellationToken = default);
    Task<bool> ExisteCodigoAsync(CodigoCredencial codigo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Credencial>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Credencial credencial, CancellationToken cancellationToken = default);
}
