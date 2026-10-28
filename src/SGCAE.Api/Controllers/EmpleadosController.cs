using Microsoft.AspNetCore.Mvc;
using SGCAE.Application;
using SGCAE.Application.Commands;
using SGCAE.Application.Common;
using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Queries;
using MediatR;

namespace SGCAE.Api.Controllers;

/// <summary>RF-01: gestión de empleados.</summary>
[ApiController]
[Route("api/empleados")]
[Produces("application/json")]
public sealed class EmpleadosController(ISender mediator) : ControllerBase
{
    /// <summary>Registra un nuevo empleado con vínculo laboral activo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmpleadoDto>> Registrar(
        [FromBody] RegistrarEmpleadoCommand comando, CancellationToken token)
    {
        var resultado = await mediator.Send(comando, token);
        return CreatedAtAction(nameof(ObtenerPorId), new { empleadoId = resultado.Id }, resultado);
    }

    /// <summary>Lista empleados con filtros y paginación.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginadoDto<EmpleadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginadoDto<EmpleadoDto>>> Listar(
        [FromQuery] string? busqueda, [FromQuery] Domain.Enums.EstadoLaboral? estado,
        [FromQuery] int pagina = 1, [FromQuery] int tamanoPagina = 10, CancellationToken token = default)
    {
        return Ok(await mediator.Send(
            new ListarEmpleadosQuery(busqueda, estado, pagina, tamanoPagina), token));
    }

    /// <summary>Obtiene un empleado por su identificador.</summary>
    [HttpGet("{empleadoId:guid}")]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpleadoDto>> ObtenerPorId(Guid empleadoId, CancellationToken token)
        => Ok(await mediator.Send(new ObtenerEmpleadoPorIdQuery(empleadoId), token));

    /// <summary>Actualiza los datos de un empleado.</summary>
    [HttpPut("{empleadoId:guid}")]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmpleadoDto>> Actualizar(
        Guid empleadoId, [FromBody] ActualizarEmpleadoCommand comando, CancellationToken token)
        => Ok(await mediator.Send(comando with { EmpleadoId = empleadoId }, token));

    /// <summary>Desactiva el vínculo laboral; revoca sus credenciales activas.</summary>
    [HttpDelete("{empleadoId:guid}")]
    [ProducesResponseType(typeof(EmpleadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpleadoDto>> Desactivar(Guid empleadoId, CancellationToken token)
        => Ok(await mediator.Send(new DesactivarEmpleadoCommand(empleadoId), token));
}
