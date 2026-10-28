using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using SGCAE.Domain.Enums;
using SGCAE.Persistence.Abstractions;
using MediatR;

namespace SGCAE.Application.Queries;

/// <summary>RF-04: consulta paginada del historial de accesos con filtros.</summary>
public sealed class ConsultarHistorialAccesosHandler(ISgcaeReadRepository db)
    : IRequestHandler<ConsultarHistorialAccesosQuery, PaginadoDto<RegistroAccesoDto>>
{
    public async Task<PaginadoDto<RegistroAccesoDto>> Handle(
        ConsultarHistorialAccesosQuery query, CancellationToken cancellationToken)
    {
        var pagina = Math.Max(query.Pagina, 1);
        var tamano = Math.Clamp(query.TamanoPagina, 1, 100);

        var (desde, hasta) = NormalizarRangoFechas(query.FechaDesde, query.FechaHasta);

        var filtrados = db.RegistrosAcceso.AsNoTracking();

        if (query.EmpleadoId is not null)
            filtrados = filtrados.Where(r => r.EmpleadoId == query.EmpleadoId);

        if (query.PuntoAccesoId is not null)
            filtrados = filtrados.Where(r => r.PuntoAccesoId == query.PuntoAccesoId);

        if (desde is not null)
            filtrados = filtrados.Where(r => r.FechaHora >= desde);

        if (hasta is not null)
            filtrados = filtrados.Where(r => r.FechaHora < hasta);

        if (query.Resultado is not null)
            filtrados = filtrados.Where(r => r.Resultado == query.Resultado);

        if (query.TipoMovimiento is not null)
            filtrados = filtrados.Where(r => r.TipoMovimiento == query.TipoMovimiento);

        var total = await filtrados.CountAsync(cancellationToken);

        var elementos = await filtrados
            .OrderByDescending(r => r.FechaHora)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Join(db.Empleados.AsNoTracking(),
                r => r.EmpleadoId, e => (Guid?)e.Id,
                (r, e) => new { r, NombreEmpleado = e.Nombres + " " + e.Apellidos })
            .Join(db.PuntosAcceso.AsNoTracking(),
                rx => rx.r.PuntoAccesoId, p => p.Id,
                (rx, p) => new RegistroAccesoDto(
                    rx.r.Id, rx.r.FechaHora, rx.r.EmpleadoId, rx.NombreEmpleado,
                    rx.r.CodigoUtilizado, p.Id, p.Nombre,
                    rx.r.TipoMovimiento, rx.r.Resultado, rx.r.MotivoRechazo))
            .ToListAsync(cancellationToken);

        return new PaginadoDto<RegistroAccesoDto>(elementos, pagina, tamano, total);
    }

    private static (DateTime? Desde, DateTime? Hasta) NormalizarRangoFechas(DateTime? desde, DateTime? hasta)
    {
        // FechaHasta es inclusiva en la interfaz: se desplaza al inicio del día siguiente.
        return (desde, hasta?.Date.AddDays(1));
    }
}

/// <summary>RF-02: lista credenciales con filtros por empleado y estado.</summary>
public sealed class ListarCredencialesHandler(ISgcaeReadRepository db)
    : IRequestHandler<ListarCredencialesQuery, IReadOnlyList<CredencialDto>>
{
    public async Task<IReadOnlyList<CredencialDto>> Handle(
        ListarCredencialesQuery query, CancellationToken cancellationToken)
    {
        var consulta = from credencial in db.Credenciales.AsNoTracking()
                       join empleado in db.Empleados.AsNoTracking()
                           on credencial.EmpleadoId equals empleado.Id
                       select new { credencial, nombreEmpleado = empleado.Nombres + " " + empleado.Apellidos };

        if (query.EmpleadoId is not null)
            consulta = consulta.Where(x => x.credencial.EmpleadoId == query.EmpleadoId);

        if (query.Estado is not null)
            consulta = consulta.Where(x => x.credencial.Estado == query.Estado);

        var resultado = await consulta
            .OrderBy(x => x.credencial.FechaEmision)
            .Select(x => new CredencialDto(
                x.credencial.Id, x.credencial.EmpleadoId, x.nombreEmpleado.Trim(),
                x.credencial.Codigo.Valor, x.credencial.Estado, x.credencial.FechaEmision))
            .ToListAsync(cancellationToken);

        return resultado;
    }
}

/// <summary>RF-02: obtiene una credencial por su identificador.</summary>
public sealed class ObtenerCredencialPorIdHandler(ISgcaeReadRepository db)
    : IRequestHandler<ObtenerCredencialPorIdQuery, CredencialDto>
{
    public async Task<CredencialDto> Handle(ObtenerCredencialPorIdQuery query, CancellationToken cancellationToken)
    {
        var dto = await (from credencial in db.Credenciales.AsNoTracking()
                         join empleado in db.Empleados.AsNoTracking() on credencial.EmpleadoId equals empleado.Id
                         where credencial.Id == query.CredencialId
                         select new CredencialDto(
                             credencial.Id, credencial.EmpleadoId,
                             (empleado.Nombres + " " + empleado.Apellidos).Trim(),
                             credencial.Codigo.Valor, credencial.Estado, credencial.FechaEmision))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("credencial", query.CredencialId);

        return dto;
    }
}

/// <summary>RF-06: lista los puntos de acceso (opcionalmente solo habilitados).</summary>
public sealed class ListarPuntosAccesoHandler(ISgcaeReadRepository db)
    : IRequestHandler<ListarPuntosAccesoQuery, IReadOnlyList<PuntoAccesoDto>>
{
    public async Task<IReadOnlyList<PuntoAccesoDto>> Handle(
        ListarPuntosAccesoQuery query, CancellationToken cancellationToken)
    {
        var consulta = db.PuntosAcceso.AsNoTracking();

        if (query.SoloHabilitados is not null)
            consulta = consulta.Where(p => p.Habilitado == query.SoloHabilitados);

        var resultado = await consulta
            .OrderBy(p => p.Nombre)
            .Select(p => new PuntoAccesoDto(p.Id, p.Nombre, p.Descripcion, p.Habilitado))
            .ToListAsync(cancellationToken);

        return resultado;
    }
}
