using System.Text.RegularExpressions;
using SGCAE.Domain.Common;

namespace SGCAE.Domain.ValueObjects;

/// <summary>
/// Objeto de valor: correo electrónico del empleado.
/// Garantiza formato válido y normalización a minúsculas.
/// </summary>
public sealed partial class CorreoElectronico : ValueObject
{
    private static readonly Regex Patron =
        new(@"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Valor { get; private set; } = default!;

    private CorreoElectronico() { }

    private CorreoElectronico(string valor) => Valor = valor;

    /// <exception cref="Exceptions.DomainException">Si el formato no es válido.</exception>
    public static CorreoElectronico Crear(string? valor)
    {
        var normalizado = valor?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizado))
            throw new DomainException("El correo electrónico es obligatorio.");

        if (normalizado.Length > 256)
            throw new DomainException("El correo electrónico no debe exceder 256 caracteres.");

        if (!Patron.IsMatch(normalizado))
            throw new DomainException($"El correo electrónico '{normalizado}' no es válido.");

        return new CorreoElectronico(normalizado);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    public static explicit operator string(CorreoElectronico email) => email.Valor;
}
