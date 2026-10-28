using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using SGCAE.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SGCAE.Infrastructure.Persistence.Repositories;

internal sealed class CredencialRepository(SgcaeDbContext context) : ICredencialRepository
{
    public async Task<Credencial?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Credenciales.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Credencial?> ObtenerPorCodigoAsync(CodigoCredencial codigo, CancellationToken cancellationToken = default)
        => await context.Credenciales.SingleOrDefaultAsync(c => c.Codigo.Valor == codigo.Valor, cancellationToken);

    public async Task<bool> ExisteCodigoAsync(CodigoCredencial codigo, CancellationToken cancellationToken = default)
        => await context.Credenciales.AnyAsync(c => c.Codigo.Valor == codigo.Valor, cancellationToken);

    public async Task<IReadOnlyList<Credencial>> ListarPorEmpleadoAsync(
        Guid empleadoId, CancellationToken cancellationToken = default)
    {
        var resultado = await context.Credenciales
            .Where(c => c.EmpleadoId == empleadoId)
            .OrderBy(c => c.FechaEmision)
            .ToListAsync(cancellationToken);

        return resultado.AsReadOnly();
    }

    public async Task AgregarAsync(Credencial credencial, CancellationToken cancellationToken = default)
        => await context.Credenciales.AddAsync(credencial, cancellationToken);
}
