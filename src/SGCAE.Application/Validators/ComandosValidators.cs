using FluentValidation;
using SGCAE.Application.Commands;

namespace SGCAE.Application.Validators;

public sealed class ReactivarCredencialValidator : AbstractValidator<ReactivarCredencialCommand>
{
    public ReactivarCredencialValidator() => RuleFor(c => c.CredencialId).NotEmpty();
}

public sealed class RevocarCredencialValidator : AbstractValidator<RevocarCredencialCommand>
{
    public RevocarCredencialValidator() => RuleFor(c => c.CredencialId).NotEmpty();
}

public sealed class RegistrarPuntoAccesoValidator : AbstractValidator<RegistrarPuntoAccesoCommand>
{
    public RegistrarPuntoAccesoValidator()
    {
        RuleFor(c => c.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Descripcion).NotEmpty().MaximumLength(250);
    }
}

public sealed class RegistrarSolicitudAccesoValidator : AbstractValidator<RegistrarSolicitudAccesoCommand>
{
    public RegistrarSolicitudAccesoValidator()
    {
        RuleFor(c => c.CodigoCredencial)
            .NotEmpty()
            .Length(6, 20)
            .Matches("^[A-Za-z0-9]+$")
            .WithMessage("El código de credencial solo admite letras y dígitos sin espacios.");

        RuleFor(c => c.PuntoAccesoId).NotEmpty();

        RuleFor(c => c.TipoMovimiento)
            .IsInEnum()
            .WithMessage("El tipo de movimiento debe ser Entrada o Salida.");
    }
}

public sealed class HabilitarPuntoAccesoValidator : AbstractValidator<HabilitarPuntoAccesoCommand>
{
    public HabilitarPuntoAccesoValidator() => RuleFor(c => c.PuntoAccesoId).NotEmpty();
}

public sealed class InhabilitarPuntoAccesoValidator : AbstractValidator<InhabilitarPuntoAccesoCommand>
{
    public InhabilitarPuntoAccesoValidator() => RuleFor(c => c.PuntoAccesoId).NotEmpty();
}
