using FluentValidation;
using MediatR;
using SGCAE.Application.Common.Exceptions;

namespace SGCAE.Application.Behaviors;

/// <summary>
/// Pipeline de MediatR: valida cada comando/consulta con sus reglas de
/// FluentValidation antes de que el handler se ejecute.
/// </summary>
public sealed class ValidacionComportamiento<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validadores)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validadores.Any())
            return await next();

        var contexto = new ValidationContext<TRequest>(request);

        var resultados = await Task.WhenAll(
            validadores.Select(v => v.ValidateAsync(contexto, cancellationToken)));

        var errores = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
            .Distinct()
            .ToArray();

        if (errores.Length is not 0)
            throw new ValidacionException(errores);

        return await next();
    }
}
