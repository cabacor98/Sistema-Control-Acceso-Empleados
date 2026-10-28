using SGCAE.Application.Common;
using SGCAE.Application.Common.Dtos;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Application.Commands;

public sealed record RegistrarEmpleadoCommand(
    string Nombres,
    string Apellidos,
    string Email,
    string Cargo,
    string AreaOrganizacional) : ICommand<EmpleadoDto>;

public sealed record ActualizarEmpleadoCommand(
    Guid EmpleadoId,
    string Nombres,
    string Apellidos,
    string Email,
    string Cargo,
    string AreaOrganizacional) : ICommand<EmpleadoDto>;

public sealed record DesactivarEmpleadoCommand(Guid EmpleadoId) : ICommand<EmpleadoDto>;

public sealed record AsignarCredencialCommand(Guid EmpleadoId, string Codigo) : ICommand<CredencialDto>;

public sealed record BloquearCredencialCommand(Guid CredencialId) : ICommand<CredencialDto>;

public sealed record ReactivarCredencialCommand(Guid CredencialId) : ICommand<CredencialDto>;

public sealed record RevocarCredencialCommand(Guid CredencialId) : ICommand<CredencialDto>;
