using SGCAE.Domain.Entities;

namespace SGCAE.Domain.Repositories;

/// <summary>Puerto de persistencia del agregado RegistroAcceso.</summary>
public interface IRegistroAccesoRepository
{
    Task AgregarAsync(RegistroAcceso registro, CancellationToken cancellationToken = default);

    /// <summary>Determina si existe una entrada autorizada sin salida posterior (persona "dentro").</summary>
    Task<bool> TieneEntradaAbiertaAsync(Guid empleadoId, CancellationToken cancellationToken = default);
}
