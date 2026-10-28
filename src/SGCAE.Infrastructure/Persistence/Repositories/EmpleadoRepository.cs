using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SGCAE.Infrastructure.Persistence.Repositories;

internal sealed class EmpleadoRepository(SgcaeDbContext context) : IEmpleadoRepository
{
    public async Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Empleados
            .Include(e => e.Credenciales)
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<bool> ExisteEmailAsync(string email, Guid? idExcluido = null, CancellationToken cancellationToken = default)
    {
        var consulta = context.Empleados.Where(e => e.Email.Valor == email);

        if (idExcluido is not null)
            consulta = consulta.Where(e => e.Id != idExcluido);

        return await consulta.AnyAsync(cancellationToken);
    }

    public async Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken = default)
        => await context.Empleados.AddAsync(empleado, cancellationToken);
}
