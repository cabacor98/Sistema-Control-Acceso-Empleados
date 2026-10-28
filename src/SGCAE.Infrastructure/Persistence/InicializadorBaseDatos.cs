using SGCAE.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SGCAE.Infrastructure.Persistence;

/// <summary>
/// Aplica migraciones automáticas al arrancar (LocalDB) y siembra tres
/// empleados de demostración con credenciales para evidenciar accesos
/// autorizados y rechazados. Idempotente: solo siembra si no hay empleados.
/// </summary>
public static class InicializadorBaseDatos
{
    public static async Task EjecutarAsync(IServiceProvider serviciosRaiz)
    {
        using var alcance = serviciosRaiz.CreateScope();
        var servicios = alcance.ServiceProvider;

        var context = servicios.GetRequiredService<SgcaeDbContext>();
        await context.Database.MigrateAsync();

        await SembrarDemoAsync(servicios, context);
    }

    private static async Task SembrarDemoAsync(IServiceProvider servicios, SgcaeDbContext context)
    {
        if (await context.Empleados.AnyAsync())
            return;

        var mediator = servicios.GetRequiredService<ISender>();

        // Empleado activo con credencial vigente.
        var activa = await mediator.Send(new RegistrarEmpleadoCommand(
            "María Fernanda", "Ortega Ruiz", "m.ortega@logistica-demo.com",
            "Coordinadora de Almacén", "Operaciones"), CancellationToken.None);

        await mediator.Send(new AsignarCredencialCommand(
            activa.Id, "CRED1077"), CancellationToken.None);

        // Empleado activo sin credencial (para asignar en la demo).
        await mediator.Send(new RegistrarEmpleadoCommand(
            "Andrés Iván", "Méndez Torres", "a.mendez@logistica-demo.com",
            "Analista de Inventarios", "Logística"), CancellationToken.None);

        // Empleado inactivo: su credencial se revoca automáticamente al desactivar.
        var inactivo = await mediator.Send(new RegistrarEmpleadoCommand(
            "Camilo", "Restrepo Vargas", "c.restrepo@logistica-demo.com",
            "Auxiliar de Acomodo", "Operaciones"), CancellationToken.None);

        var credencialInactiva = await mediator.Send(new AsignarCredencialCommand(
            inactivo.Id, "CRED2056"), CancellationToken.None);

        await mediator.Send(new DesactivarEmpleadoCommand(
            credencialInactiva.EmpleadoId), CancellationToken.None);
    }
}
