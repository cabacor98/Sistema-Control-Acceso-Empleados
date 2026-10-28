using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Commands;
using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Queries;
using MediatR;

namespace SGCAE.Api.Controllers;

/// <summary>RF-06: administración de puntos de acceso físicos.</summary>
[ApiController]
[Route("api/puntos-acceso")]
[Produces("application/json")]
public sealed class PuntosAccesoController(ISender mediator) : ControllerBase
{
    /// <summary>Registra un nuevo punto de acceso.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PuntoAccesoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PuntoAccesoDto>> Registrar(
        [FromBody] RegistrarPuntoAccesoCommand comando, CancellationToken token)
    {
        var resultado = await mediator.Send(comando, token);
        return Created($"/api/puntos-acceso/{resultado.Id}", resultado);
    }

    /// <summary>Lista los puntos de acceso.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PuntoAccesoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PuntoAccesoDto>>> Listar(
        [FromQuery] bool? soloHabilitados, CancellationToken token)
        => Ok(await mediator.Send(new ListarPuntosAccesoQuery(soloHabilitados), token));

    /// <summary>Habilita un punto de acceso.</summary>
    [HttpPost("{puntoAccesoId:guid}/habilitar")]
    [ProducesResponseType(typeof(PuntoAccesoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PuntoAccesoDto>> Habilitar(Guid puntoAccesoId, CancellationToken token)
        => Ok(await mediator.Send(new HabilitarPuntoAccesoCommand(puntoAccesoId), token));

    /// <summary>Inhabilita un punto de acceso.</summary>
    [HttpPost("{puntoAccesoId:guid}/inhabilitar")]
    [ProducesResponseType(typeof(PuntoAccesoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PuntoAccesoDto>> Inhabilitar(Guid puntoAccesoId, CancellationToken token)
        => Ok(await mediator.Send(new InhabilitarPuntoAccesoCommand(puntoAccesoId), token));
}
