namespace SGCAE.Application.Common.Exceptions;

/// <summary>
/// Errores de validación de entrada (400 Bad Request).
/// </summary>
public sealed class ValidacionException(string[] errores)
    : AplicacionException("La solicitud contiene datos inválidos.")
{
    public IReadOnlyDictionary<string, string[]> Errores { get; } = errores
        .GroupBy(e => e[..e.IndexOf(':')])
        .ToDictionary(g => g.Key, g => g.Select(x => x[(x.IndexOf(':') + 2)..]).ToArray(), StringComparer.OrdinalIgnoreCase);
}
