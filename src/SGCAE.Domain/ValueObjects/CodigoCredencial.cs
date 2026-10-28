using SGCAE.Domain.Common;

namespace SGCAE.Domain.ValueObjects;

/// <summary>
/// Objeto de valor: código identificador de una credencial de acceso.
/// Entre 6 y 20 caracteres alfanuméricos (letras y dígitos), sin espacios.
/// Se normaliza a mayúsculas.
/// </summary>
public sealed class CodigoCredencial : ValueObject
{
    public const int LongitudMinima = 6;
    public const int LongitudMaxima = 20;

    public string Valor { get; private set; } = default!;

    private CodigoCredencial() { }

    private CodigoCredencial(string valor) => Valor = valor;

    /// <exception cref="Exceptions.DomainException">Si el formato no es válido.</exception>
    public static CodigoCredencial Crear(string? valor)
    {
        if (!TryCrear(valor, out var codigo))
            throw new Exceptions.DomainException(ObtenerError(valor));

        return codigo!;
    }

    public static bool TryCrear(string? valor, out CodigoCredencial? codigo)
    {
        codigo = null;

        var normalizado = valor?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizado))
            return false;

        if (normalizado.Length < LongitudMinima || normalizado.Length > LongitudMaxima)
            return false;

        if (!normalizado.All(char.IsLetterOrDigit))
            return false;

        codigo = new CodigoCredencial(normalizado);
        return true;
    }

    private static string ObtenerError(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return "El código de la credencial es obligatorio.";

        var contieneEspacios = valor.AsSpan().TrimStart().Length != valor.Length
            || valor.AsSpan().TrimEnd().Length != valor.Length;

        var longitud = valor.Trim().Length;

        if (contieneEspacios)
            return "El código de la credencial no admite espacios.";

        if (longitud < LongitudMinima || longitud > LongitudMaxima)
            return $"El código de la credencial debe tener entre {LongitudMinima} y {LongitudMaxima} caracteres.";

        return "El código de la credencial solo admite letras y dígitos.";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    public static explicit operator string(CodigoCredencial codigo) => codigo.Valor;
}
