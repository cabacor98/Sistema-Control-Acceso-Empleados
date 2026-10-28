namespace SGCAE.Application.Common.Exceptions;

/// <summary>
/// La operación no puede realizarse porque entra en conflicto con el estado
/// actual de los datos (p. ej. email o código duplicado).
/// </summary>
public sealed class ConflictoException(string message) : AplicacionException(message);
