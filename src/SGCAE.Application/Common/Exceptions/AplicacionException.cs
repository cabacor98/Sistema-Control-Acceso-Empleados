namespace SGCAE.Application.Common.Exceptions;

/// <summary>
/// Excepción base de la capa de aplicación.
/// </summary>
public class AplicacionException(string message) : Exception(message);
