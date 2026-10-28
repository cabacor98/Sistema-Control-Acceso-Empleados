using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using SGCAE.Domain.Services;
using SGCAE.Domain.ValueObjects;
using MediatR;

namespace SGCAE.Application.Commands;

/// <summary>
/// Orquesta la validación de una solicitud de acceso (RF-03):
/// recopila el estado de los agregados, delega la decisión al servicio
/// de dominio <see cref="ValidadorDeAcceso"/> y persiste el registro
/// del movimiento, se autorice o se rechace.
/// </summary>
public sealed class RegistrarSolicitudAccesoHandler(
    ICredencialRepository credenciales,
    IEmpleadoRepository empleados,
    IPuntoAccesoRepository puntosAcceso,
    IRegistroAccesoRepository registrosAcceso,
    IUnitOfWork unitOfWork,
    ValidadorDeAcceso validadorDeAcceso) : IRequestHandler<RegistrarSolicitudAccesoCommand, SolicitudAccesoDto>
{
    public async Task<SolicitudAccesoDto> Handle(RegistrarSolicitudAccesoCommand command, CancellationToken cancellationToken)
    {
        var codigoUtilizado = CodigoCredencial.Crear(command.CodigoCredencial);
        var credencial = await credenciales.ObtenerPorCodigoAsync(codigoUtilizado, cancellationToken);
        var punto = await puntosAcceso.ObtenerPorIdAsync(command.PuntoAccesoId, cancellationToken);

        // Empleado se obtiene por la credencial localizada por código para no
        // depender de un identificador externo que podría estar desactualizado.
        var empleado = credencial is not null
            ? await empleados.ObtenerPorIdAsync(credencial.EmpleadoId, cancellationToken)
            : null;

        var tieneEntradaAbierta = credencial is not null
            && await registrosAcceso.TieneEntradaAbiertaAsync(credencial.EmpleadoId, cancellationToken);

        var momentoActual = DateTime.UtcNow;

        var resultado = validadorDeAcceso.Validar(
            empleado, credencial, codigoUtilizado, punto,
            command.TipoMovimiento, tieneEntradaAbierta, momentoActual);

        var registro = RegistroAcceso.Registrar(
            momentoActual, resultado.EmpleadoId, resultado.CredencialId,
            codigoUtilizado, command.PuntoAccesoId, command.TipoMovimiento, resultado);

        await registrosAcceso.AgregarAsync(registro, cancellationToken);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return new SolicitudAccesoDto(
            registro.Id,
            registro.Resultado == Domain.Enums.ResultadoAcceso.Autorizado,
            registro.MotivoRechazo,
            resultado switch
            {
                { EsAutorizado: true } => $"{resultado.NombresEmpleado} {resultado.ApellidosEmpleado}".Trim(),
                _ => null
            },
            punto?.Id ?? command.PuntoAccesoId,
            punto?.Nombre ?? string.Empty,
            command.TipoMovimiento,
            registro.FechaHora);
    }
}
