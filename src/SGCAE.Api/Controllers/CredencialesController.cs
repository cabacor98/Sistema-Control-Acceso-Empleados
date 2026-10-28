using Microsoft.AspNetCore.Mvc;
using SGCAE.Application.Commands;
using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Queries;
using MediatR;

namespace SGCAE.Api.Controllers;

/// <summary>RF-02: gestión de credenciales de acceso.</summary>
[ApiController]
[Route("api/credenciales")]
[Produces("application/json")]
public sealed class CredencialesController(ISender mediator) : ControllerBase
{
    /// <summary>Asigna una credencial (código único) a un empleado activo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CredencialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CredencialDto>> Asignar(
        [FromBody] AsignarCredencialCommand comando, CancellationToken token)
    {
        var resultado = await mediator.Send(comando, token);
        return CreatedAtRoute(
            $"ObtenerCredencial", routeValues: new { credencialId = resultado.Id }, value: resultado);
    }

    /// <summary>Lista credenciales con filtros por empleado y estado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CredencialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CredencialDto>>> Listar(
        [FromQuery] Guid? empleadoId, [FromQuery] Domain.Enums.EstadoCredencial? estado, CancellationToken token)
        => Ok(await mediator.Send(new ListarCredencialesQuery(empleadoId, estado), token));

    /// <summary>Obtiene una credencial por su identificador.</summary>
    [HttpGet("{credencialId:guid}", Name = "ObtenerCredencial")]
    [ProducesResponseType(typeof(CredencialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredencialDto>> Obtener(Guid credencialId, CancellationToken token)
        => Ok(await mediator.Send(new ObtenerCredencialPorIdQuery(credencialId), token));

    /// <summary>Bloquea temporalmente una credencial.</summary>
    [HttpPost("{credencialId:guid}/bloquear")]
    [ProducesResponseType(typeof(CredencialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredencialDto>> Bloquear(Guid credencialId, CancellationToken token)
        => Ok(await mediator.Send(new BloquearCredencialCommand(credencialId), token));

    /// <summary>Reactiva una credencial bloqueada.</summary>
    [HttpPost("{credencialId:guid}/reactivar")]
    [ProducesResponseType(typeof(CredencialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredencialDto>> Reactivar(Guid credencialId, CancellationToken token)
        => Ok(await mediator.Send(new ReactivarCredencialCommand(credencialId), token));

    /// <summary>Revoca de forma definitiva una credencial.</summary>
    [HttpPost("{credencialId:guid}/revocar")]
    [ProducesResponseType(typeof(CredencialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CredencialDto>> Revocar(Guid credencialId, CancellationToken token)
        => Ok(await mediator.Send(new RevocarCredencialCommand(credencialId), token));
}
