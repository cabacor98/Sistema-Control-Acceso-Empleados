using FluentValidation;
using SGCAE.Application.Commands;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Application.Validators;

public sealed class RegistrarEmpleadoValidator : AbstractValidator<RegistrarEmpleadoCommand>
{
    public RegistrarEmpleadoValidator()
    {
        RuleFor(c => c.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(c => c.Cargo).NotEmpty().MaximumLength(100);
        RuleFor(c => c.AreaOrganizacional).NotEmpty().MaximumLength(100);
    }
}

public sealed class ActualizarEmpleadoValidator : AbstractValidator<ActualizarEmpleadoCommand>
{
    public ActualizarEmpleadoValidator()
    {
        RuleFor(c => c.EmpleadoId).NotEmpty();
        RuleFor(c => c.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(c => c.Cargo).NotEmpty().MaximumLength(100);
        RuleFor(c => c.AreaOrganizacional).NotEmpty().MaximumLength(100);
    }
}

public sealed class DesactivarEmpleadoValidator : AbstractValidator<DesactivarEmpleadoCommand>
{
    public DesactivarEmpleadoValidator() => RuleFor(c => c.EmpleadoId).NotEmpty();
}

public sealed class AsignarCredencialValidator : AbstractValidator<AsignarCredencialCommand>
{
    public AsignarCredencialValidator()
    {
        RuleFor(c => c.EmpleadoId).NotEmpty();
        RuleFor(c => c.Codigo)
            .NotEmpty()
            .Length(CodigoCredencial.LongitudMinima, CodigoCredencial.LongitudMaxima)
            .Matches("^[A-Za-z0-9]+$")
            .WithMessage("El código solo admite letras y dígitos sin espacios.");
    }
}

public sealed class CredencialPorIdValidator : AbstractValidator<BloquearCredencialCommand>
{
    public CredencialPorIdValidator() => RuleFor(c => c.CredencialId).NotEmpty();
}
