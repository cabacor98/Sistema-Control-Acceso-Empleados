using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SGCAE.Infrastructure.Persistence.Repositories;

internal sealed class RegistroAccesoRepository(SgcaeDbContext context) : IRegistroAccesoRepository
{
    public async Task AgregarAsync(RegistroAcceso registro, CancellationToken cancellationToken = default)
        => await context.RegistrosAcceso.AddAsync(registro, cancellationToken);

    /// <summary>
    /// Identifica si la persona sigue "dentro": su último movimiento autorizado
    /// (cualquier punto habilitado) es una entrada sin salida posterior.
    /// </summary>
    public async Task<bool> TieneEntradaAbiertaAsync(Guid empleadoId, CancellationToken cancellationToken = default)
    {
        var ultimoMovimiento = await context.RegistrosAcceso
            .Where(r => r.EmpleadoId == empleadoId && r.Resultado == ResultadoAcceso.Autorizado)
            .OrderByDescending(r => r.FechaHora)
            .Select(r => (TipoMovimiento?)r.TipoMovimiento)
            .FirstOrDefaultAsync(cancellationToken);

        return ultimoMovimiento == TipoMovimiento.Entrada;
    }
}
