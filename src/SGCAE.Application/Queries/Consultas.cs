using SGCAE.Application.Common;
using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Domain.Enums;
using SGCAE.Persistence.Abstractions;
using MediatR;

namespace SGCAE.Application.Queries;

public sealed record ListarEmpleadosQuery(
    string? Busqueda = null,
    EstadoLaboral? Estado = null,
    int Pagina = 1,
    int TamanoPagina = 10) : IQuery<PaginadoDto<EmpleadoDto>>;

public sealed record ObtenerEmpleadoPorIdQuery(Guid EmpleadoId) : IQuery<EmpleadoDto>;

public sealed record ListarCredencialesQuery(
    Guid? EmpleadoId = null,
    EstadoCredencial? Estado = null) : IQuery<IReadOnlyList<CredencialDto>>;

public sealed record ObtenerCredencialPorIdQuery(Guid CredencialId) : IQuery<CredencialDto>;

public sealed record ListarPuntosAccesoQuery(bool? SoloHabilitados = null) : IQuery<IReadOnlyList<PuntoAccesoDto>>;

public sealed record ConsultarHistorialAccesosQuery(
    Guid? EmpleadoId = null,
    Guid? PuntoAccesoId = null,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null,
    ResultadoAcceso? Resultado = null,
    TipoMovimiento? TipoMovimiento = null,
    int Pagina = 1,
    int TamanoPagina = 20) : IQuery<PaginadoDto<RegistroAccesoDto>>;
