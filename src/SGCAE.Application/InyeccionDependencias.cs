using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SGCAE.Application.Behaviors;
using SGCAE.Domain.Services;

namespace SGCAE.Application;

/// <summary>
/// Registro de los servicios de la capa de aplicación: MediatR, validadores
/// FluentValidation y comportamiento de validación (pipeline CQRS).
/// </summary>
public static class InyeccionDependencias
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var ensamblado = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(ensamblado);
            cfg.AddOpenBehavior(typeof(ValidacionComportamiento<,>));
        });

        services.AddValidatorsFromAssembly(ensamblado, includeInternalTypes: true);
        services.AddSingleton<ValidadorDeAcceso>();

        return services;
    }
}
