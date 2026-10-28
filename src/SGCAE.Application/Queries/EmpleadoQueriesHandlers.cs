using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Enums;
using SGCAE.Persistence.Abstractions;
using MediatR;

namespace SGCAE.Application.Queries;

/// <summary>RF-01: listado paginado y filtrable de empleados.</summary>
public sealed class ListarEmpleadosHandler(ISgcaeReadRepository db)
    : IRequestHandler<ListarEmpleadosQuery, PaginadoDto<EmpleadoDto>>
{
    public async Task<PaginadoDto<EmpleadoDto>> Handle(ListarEmpleadosQuery query, CancellationToken cancellationToken)
    {
        var pagina = Math.Max(query.Pagina, 1);
        var tamano = Math.Clamp(query.TamanoPagina, 1, 100);

        var filtrados = db.Empleados.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Busqueda))
        {
            var texto = query.Busqueda.Trim();
            filtrados = filtrados.Where(e =>
                e.Nombres.Contains(texto) ||
                e.Apellidos.Contains(texto) ||
                e.Email.Valor.Contains(texto));
        }

        if (query.Estado is not null)
            filtrados = filtrados.Where(e => e.Estado == query.Estado);

        var total = await filtrados.CountAsync(cancellationToken);

        var elementos = await filtrados
            .OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(e => new EmpleadoDto(
                e.Id, e.Nombres, e.Apellidos, e.Email.Valor, e.Cargo, e.AreaOrganizacional,
                e.Estado, e.FechaContratacion,
                e.Credenciales.Count(c => c.Estado == EstadoCredencial.Activa)))
            .ToListAsync(cancellationToken);

        return new PaginadoDto<EmpleadoDto>(elementos, pagina, tamano, total);
    }
}

/// <summary>RF-01: obtiene un empleado por su identificador.</summary>
public sealed class ObtenerEmpleadoPorIdHandler(ISgcaeReadRepository db)
    : IRequestHandler<ObtenerEmpleadoPorIdQuery, EmpleadoDto>
{
    public async Task<EmpleadoDto> Handle(ObtenerEmpleadoPorIdQuery query, CancellationToken cancellationToken)
    {
        var dto = await db.Empleados
            .Where(e => e.Id == query.EmpleadoId)
            .Select(e => new EmpleadoDto(
                e.Id, e.Nombres, e.Apellidos, e.Email.Valor, e.Cargo, e.AreaOrganizacional,
                e.Estado, e.FechaContratacion,
                e.Credenciales.Count(c => c.Estado == EstadoCredencial.Activa)))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("empleado", query.EmpleadoId);

        return dto;
    }
}
