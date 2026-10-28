using SGCAE.Domain.Entities;

namespace SGCAE.Domain.Repositories;

/// <summary>Puerto de persistencia del agregado PuntoAcceso.</summary>
public interface IPuntoAccesoRepository
{
    Task<PuntoAcceso?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PuntoAcceso>> ListarAsync(bool? soloHabilitados = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default);
    Task AgregarAsync(PuntoAcceso puntoAcceso, CancellationToken cancellationToken = default);
}
