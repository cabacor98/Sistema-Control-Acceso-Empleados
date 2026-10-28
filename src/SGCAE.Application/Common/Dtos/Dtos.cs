using SGCAE.Domain.Enums;

namespace SGCAE.Application.Common.Dtos;

/// <summary>Representación de lectura de un empleado (RF-01).</summary>
public sealed record EmpleadoDto(
    Guid Id,
    string Nombres,
    string Apellidos,
    string Email,
    string Cargo,
    string AreaOrganizacional,
    EstadoLaboral Estado,
    DateTime FechaContratacion,
    int CredencialesVigentes);

/// <summary>Representación de lectura de una credencial (RF-02).</summary>
public sealed record CredencialDto(
    Guid Id,
    Guid EmpleadoId,
    string NombreEmpleado,
    string Codigo,
    EstadoCredencial Estado,
    DateTime FechaEmision);

/// <summary>Representación de lectura de un punto de acceso (RF-06).</summary>
public sealed record PuntoAccesoDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    bool Habilitado);

/// <summary>Representación de lectura de un movimiento de acceso (RF-03 / RF-04).</summary>
public sealed record RegistroAccesoDto(
    Guid Id,
    DateTime FechaHora,
    Guid? EmpleadoId,
    string? NombreEmpleado,
    string CodigoCredencial,
    Guid PuntoAccesoId,
    string NombrePuntoAcceso,
    TipoMovimiento TipoMovimiento,
    ResultadoAcceso Resultado,
    MotivoRechazo MotivoRechazo);

/// <summary>Resultado paginado genérico para consultas (RF-04).</summary>
public sealed record PaginadoDto<T>(IReadOnlyList<T> Items, int Pagina, int TamanoPagina, int TotalElementos)
{
    public int TotalPaginas => TotalElementos is 0 ? 0 : (int)Math.Ceiling(TotalElementos / (double)TamanoPagina);
}

/// <summary>Resultado del procesamiento de una solicitud de acceso (RF-03).</summary>
public sealed record SolicitudAccesoDto(
    Guid? RegistroId,
    bool Autorizado,
    MotivoRechazo MotivoRechazo,
    string? NombreEmpleado,
    Guid PuntoAccesoId,
    string NombrePuntoAcceso,
    TipoMovimiento TipoMovimiento,
    DateTime FechaHora);
