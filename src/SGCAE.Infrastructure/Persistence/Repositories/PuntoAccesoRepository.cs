using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SGCAE.Infrastructure.Persistence.Repositories;

internal sealed class PuntoAccesoRepository(SgcaeDbContext context) : IPuntoAccesoRepository
{
    public async Task<PuntoAcceso?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.PuntosAcceso.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PuntoAcceso>> ListarAsync(
        bool? soloHabilitados = null, CancellationToken cancellationToken = default)
    {
        var consulta = context.PuntosAcceso.AsQueryable();

        if (soloHabilitados is not null)
            consulta = consulta.Where(p => p.Habilitado == soloHabilitados);

        var resultado = await consulta.OrderBy(p => p.Nombre).ToListAsync(cancellationToken);
        return resultado.AsReadOnly();
    }

    public async Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default)
        => await context.PuntosAcceso.AnyAsync(p => p.Nombre == nombre, cancellationToken);

    public async Task AgregarAsync(PuntoAcceso puntoAcceso, CancellationToken cancellationToken = default)
        => await context.PuntosAcceso.AddAsync(puntoAcceso, cancellationToken);
}
