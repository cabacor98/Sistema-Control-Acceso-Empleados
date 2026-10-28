using SGCAE.Domain.Enums;

namespace SGCAE.Domain.Services;

/// <summary>
/// Resultado inmutable de la evaluación de una solicitud de acceso.
/// </summary>
public sealed record ResultadoValidacion(
    bool EsAutorizado,
    MotivoRechazo Motivo,
    Guid? EmpleadoId,
    Guid? CredencialId,
    string? NombresEmpleado = null,
    string? ApellidosEmpleado = null)
{
    public static ResultadoValidacion Autorizado(Guid empleadoId, Guid credencialId, string nombres, string apellidos)
        => new(true, MotivoRechazo.Ninguno, empleadoId, credencialId, nombres, apellidos);

    public static ResultadoValidacion Rechazado(MotivoRechazo motivo)
        => new(false, motivo, null, null);
}

/// <summary>
/// Servicio de dominio (DDD): determina si una solicitud de acceso puede
/// autorizarse evaluando el estado del empleado, de la credencial, del punto
/// de acceso y la consistencia del movimiento (RF-03).
/// Es un objeto sin estado: recibe la información que la capa de aplicación
/// recopiló de las raíces de agregado.
/// </summary>
public sealed class ValidadorDeAcceso
{
    public ResultadoValidacion Validar(
        Entities.Empleado? empleado,
        Entities.Credencial? credencial,
        ValueObjects.CodigoCredencial codigoUtilizado,
        Entities.PuntoAcceso? puntoAcceso,
        TipoMovimiento tipoMovimiento,
        bool tieneEntradaAbierta,
        DateTime momentoActual)
    {
        ResultadoValidacion rechazo(MotivoRechazo motivo) => ResultadoValidacion.Rechazado(motivo);

        if (credencial is null)
            return rechazo(MotivoRechazo.CredencialNoRegistrada);

        if (puntoAcceso is null)
            return rechazo(MotivoRechazo.PuntoAccesoNoHabilitado);

        if (!puntoAcceso.Habilitado)
            return rechazo(MotivoRechazo.PuntoAccesoNoHabilitado);

        if (empleado is null || credencial.EmpleadoId != empleado.Id)
            return rechazo(MotivoRechazo.CredencialNoRegistrada);

        if (empleado.Estado == Enums.EstadoLaboral.Inactivo)
            return rechazo(MotivoRechazo.EmpleadoInactivo);

        if (credencial.Estado != Enums.EstadoCredencial.Activa)
            return rechazo(MotivoRechazo.CredencialNoActiva);

        if (tipoMovimiento == TipoMovimiento.Salida && !tieneEntradaAbierta)
            return rechazo(MotivoRechazo.SalidaSinEntradaPrevia);

        return ResultadoValidacion.Autorizado(empleado.Id, credencial.Id, empleado.Nombres, empleado.Apellidos);
    }
}
