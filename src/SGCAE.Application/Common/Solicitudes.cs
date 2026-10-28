namespace SGCAE.Application.Common;

/// <summary>
/// Marcador CQRS: una solicitud de modificación de estado.
/// Los comandos se nombran con imperativos (RegistrarX, ActualizarX, ...).
/// </summary>
public interface ICommand<TResultado> : MediatR.IRequest<TResultado>;

/// <summary>
/// Marcador CQRS: una solicitud de lectura que no modifica estado.
/// Las consultas se nombran con sustantivos (ObtenerX, ListarX, ...).
/// </summary>
public interface IQuery<TResultado> : MediatR.IRequest<TResultado>;
