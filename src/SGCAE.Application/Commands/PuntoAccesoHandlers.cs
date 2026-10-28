using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using MediatR;

namespace SGCAE.Application.Commands;

/// <summary>RF-06: registra un nuevo punto de acceso físico.</summary>
public sealed class RegistrarPuntoAccesoHandler(
    IPuntoAccesoRepository puntosAcceso,
    IUnitOfWork unitOfWork) : IRequestHandler<RegistrarPuntoAccesoCommand, PuntoAccesoDto>
{
    public async Task<PuntoAccesoDto> Handle(RegistrarPuntoAccesoCommand command, CancellationToken cancellationToken)
    {
        if (await puntosAcceso.ExisteNombreAsync(command.Nombre.Trim(), cancellationToken))
            throw new ConflictoException($"Ya existe un punto de acceso con el nombre '{command.Nombre}'.");

        var punto = PuntoAcceso.Registrar(command.Nombre, command.Descripcion);

        await puntosAcceso.AgregarAsync(punto, cancellationToken);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return punto.ADto();
    }
}

/// <summary>RF-06: habilita un punto de acceso.</summary>
public sealed class HabilitarPuntoAccesoHandler(
    IPuntoAccesoRepository puntosAcceso,
    IUnitOfWork unitOfWork) : IRequestHandler<HabilitarPuntoAccesoCommand, PuntoAccesoDto>
{
    public async Task<PuntoAccesoDto> Handle(HabilitarPuntoAccesoCommand command, CancellationToken cancellationToken)
    {
        var punto = await puntosAcceso.ObtenerPorIdAsync(command.PuntoAccesoId, cancellationToken)
            ?? throw new NotFoundException("punto de acceso", command.PuntoAccesoId);

        punto.Habilitar();
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return punto.ADto();
    }
}

/// <summary>RF-06: inhabilita un punto de acceso.</summary>
public sealed class InhabilitarPuntoAccesoHandler(
    IPuntoAccesoRepository puntosAcceso,
    IUnitOfWork unitOfWork) : IRequestHandler<InhabilitarPuntoAccesoCommand, PuntoAccesoDto>
{
    public async Task<PuntoAccesoDto> Handle(InhabilitarPuntoAccesoCommand command, CancellationToken cancellationToken)
    {
        var punto = await puntosAcceso.ObtenerPorIdAsync(command.PuntoAccesoId, cancellationToken)
            ?? throw new NotFoundException("punto de acceso", command.PuntoAccesoId);

        punto.Inhabilitar();
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return punto.ADto();
    }
}
