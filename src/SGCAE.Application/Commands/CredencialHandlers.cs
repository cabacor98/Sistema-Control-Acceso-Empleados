using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common.Exceptions;
using SGCAE.Domain.Repositories;
using MediatR;

namespace SGCAE.Application.Commands;

/// <summary>RF-02: reactiva una credencial bloqueada.</summary>
public sealed class ReactivarCredencialHandler(
    ICredencialRepository credenciales,
    IUnitOfWork unitOfWork) : IRequestHandler<ReactivarCredencialCommand, CredencialDto>
{
    public async Task<CredencialDto> Handle(ReactivarCredencialCommand command, CancellationToken cancellationToken)
    {
        var credencial = await credenciales.ObtenerPorIdAsync(command.CredencialId, cancellationToken)
            ?? throw new NotFoundException("credencial", command.CredencialId);

        credencial.Reactivar();
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return credencial.ADto(string.Empty);
    }
}

/// <summary>RF-02: revoca de forma definitiva una credencial.</summary>
public sealed class RevocarCredencialHandler(
    ICredencialRepository credenciales,
    IUnitOfWork unitOfWork) : IRequestHandler<RevocarCredencialCommand, CredencialDto>
{
    public async Task<CredencialDto> Handle(RevocarCredencialCommand command, CancellationToken cancellationToken)
    {
        var credencial = await credenciales.ObtenerPorIdAsync(command.CredencialId, cancellationToken)
            ?? throw new NotFoundException("credencial", command.CredencialId);

        credencial.Revocar();
        await unitOfWork.GuardarCambiosAsync(cancellationToken);

        return credencial.ADto(string.Empty);
    }
}
