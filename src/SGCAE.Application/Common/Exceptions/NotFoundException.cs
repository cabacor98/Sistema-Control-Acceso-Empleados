namespace SGCAE.Application.Common.Exceptions;

/// <summary>El recurso solicitado no existe.</summary>
public sealed class NotFoundException(string recurso, object clave)
    : AplicacionException($"El {recurso} con identificador '{clave}' no existe.");
