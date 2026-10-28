namespace SGCAE.Domain.Exceptions;

/// <summary>
/// Excepción base para todas las violaciones de reglas del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
