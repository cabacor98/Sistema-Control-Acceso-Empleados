using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Commands;
using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Queries;
using MediatR;

namespace SGCAE.Api.Controllers;

/// <summary>RF-03 y RF-04: solicitudes de acceso y consultas del historial.</summary>
[ApiController]
[Route("api/accesos")]
[Produces("application/json")]
public sealed class AccesosController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Registra una solicitud de acceso (entrada o salida). El sistema valida
    /// las condiciones del empleado y registra el resultado, incluyendo rechazos.
    /// </summary>
    [HttpPost("solicitudes")]
    [ProducesResponseType(typeof(SolicitudAccesoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SolicitudAccesoDto>> RegistrarSolicitud(
        [FromBody] RegistrarSolicitudAccesoCommand comando, CancellationToken token)
    {
        var resultado = await mediator.Send(comando, token);

        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    /// <summary>Consulta el historial de accesos con filtros y paginación (RF-04).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginadoDto<RegistroAccesoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginadoDto<RegistroAccesoDto>>> Historial(
        [FromQuery] Guid? empleadoId,
        [FromQuery] Guid? puntoAccesoId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] Domain.Enums.ResultadoAcceso? resultado,
        [FromQuery] Domain.Enums.TipoMovimiento? tipoMovimiento,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        CancellationToken token = default)
        => Ok(await mediator.Send(new ConsultarHistorialAccesosQuery(
            empleadoId, puntoAccesoId, fechaDesde, fechaHasta,
            resultado, tipoMovimiento, pagina, tamanoPagina), token));
}
