using SGCAE.Application.Common;
using SGCAE.Application.Common.Dtos;
using SGCAE.Domain.Repositories;

namespace SGCAE.Application.Commands;

/// <summary>
/// RF-03: el lector/dispositivo envía una solicitud de acceso.
/// El sistema evalúa las condiciones del empleado y registra el resultado
/// (autorizado o rechazado, con motivo) en el historial trazable.
/// </summary>
public sealed record RegistrarSolicitudAccesoCommand(
    string CodigoCredencial,
    Guid PuntoAccesoId,
    Domain.Enums.TipoMovimiento TipoMovimiento) : ICommand<SolicitudAccesoDto>;
