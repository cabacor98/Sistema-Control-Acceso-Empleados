using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using SGCAE.Domain.ValueObjects;
using MediatR;

namespace SGCAE.Application.Commands;

/// <summary>RF-01: registra un nuevo empleado con vínculo laboral activo.</summary>
public sealed class RegistrarEmpleadoHandler(
    IEmpleadoRepository empleados,
    IUnitOfWork unitOfWork) : IRequestHandler<RegistrarEmpleadoCommand, EmpleadoDto>
{
    public async Task<EmpleadoDto> Handle(RegistrarEmpleadoCommand command, CancellationToken cancellationToken)
    {
        var emailNormalizado = command.Email.Trim().ToLowerInvariant();

        if (await empleados.ExisteEmailAsync(emailNormalizado, cancellationToken: cancellationToken))
            throw new ConflictoException($"Ya existe un empleado registrado con el correo '{command.Email}'.");

        var empleado = Empleado.Registrar(
            command.Nombres, command.Apellidos, command.Email,
            command.Cargo, command.AreaOrganizacional, DateTime.UtcNow);

        await empleados.AgregarAsync(empleado, cancellationToken);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return empleado.ADto();
    }
}

/// <summary>RF-01: actualiza los datos de un empleado existente.</summary>
public sealed class ActualizarEmpleadoHandler(
    IEmpleadoRepository empleados,
    IUnitOfWork unitOfWork) : IRequestHandler<ActualizarEmpleadoCommand, EmpleadoDto>
{
    public async Task<EmpleadoDto> Handle(ActualizarEmpleadoCommand command, CancellationToken cancellationToken)
    {
        var emailNormalizado = command.Email.Trim().ToLowerInvariant();

        if (await empleados.ExisteEmailAsync(emailNormalizado, command.EmpleadoId, cancellationToken))
            throw new ConflictoException($"Otro empleado ya utiliza el correo '{command.Email}'.");

        var empleado = await empleados.ObtenerPorIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException("empleado", command.EmpleadoId);

        empleado.ActualizarDatos(command.Nombres, command.Apellidos, command.Email,
            command.Cargo, command.AreaOrganizacional);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return empleado.ADto();
    }
}

/// <summary>RF-01: desactiva el vínculo laboral; sus credenciales activas se revocan automáticamente.</summary>
public sealed class DesactivarEmpleadoHandler(
    IEmpleadoRepository empleados,
    IUnitOfWork unitOfWork) : IRequestHandler<DesactivarEmpleadoCommand, EmpleadoDto>
{
    public async Task<EmpleadoDto> Handle(DesactivarEmpleadoCommand command, CancellationToken cancellationToken)
    {
        var empleado = await empleados.ObtenerPorIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException("empleado", command.EmpleadoId);

        empleado.Desactivar();
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return empleado.ADto();
    }
}

/// <summary>RF-02: asigna una nueva credencial a un empleado activo.</summary>
public sealed class AsignarCredencialHandler(
    IEmpleadoRepository empleados,
    ICredencialRepository credenciales,
    IUnitOfWork unitOfWork) : IRequestHandler<AsignarCredencialCommand, CredencialDto>
{
    public async Task<CredencialDto> Handle(AsignarCredencialCommand command, CancellationToken cancellationToken)
    {
        var empleado = await empleados.ObtenerPorIdAsync(command.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException("empleado", command.EmpleadoId);

        var codigo = CodigoCredencial.Crear(command.Codigo);

        if (await credenciales.ExisteCodigoAsync(codigo, cancellationToken))
            throw new ConflictoException($"El código de credencial '{codigo.Valor}' ya está asignado.");

        var credencial = empleado.AsignarCredencial(codigo);

        await credenciales.AgregarAsync(credencial, cancellationToken);
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return credencial.ADto($"{empleado.Nombres} {empleado.Apellidos}".Trim());
    }
}

/// <summary>RF-02: bloquea temporalmente una credencial.</summary>
public sealed class BloquearCredencialHandler(
    ICredencialRepository credenciales,
    IEmpleadoRepository empleados,
    IUnitOfWork unitOfWork) : IRequestHandler<BloquearCredencialCommand, CredencialDto>
{
    public async Task<CredencialDto> Handle(BloquearCredencialCommand command, CancellationToken cancellationToken)
    {
        var credencial = await credenciales.ObtenerPorIdAsync(command.CredencialId, cancellationToken)
            ?? throw new NotFoundException("credencial", command.CredencialId);

        credencial.Bloquear();
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return credencial.ADto(await NombreDelEmpleadoAsync(credencial.EmpleadoId, cancellationToken));
    }

    private async Task<string> NombreDelEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken)
    {
        var empleado = await empleados.ObtenerPorIdAsync(empleadoId, cancellationToken);
        return empleado is null ? string.Empty : $"{empleado.Nombres} {empleado.Apellidos}".Trim();
    }
}
