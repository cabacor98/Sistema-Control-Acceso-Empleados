namespace SGCAE.Domain.Enums;

/// <summary>
/// Resultado de la validación de una solicitud de acceso.
/// </summary>
public enum ResultadoAcceso
{
    Autorizado = 1,
    Rechazado = 2
}

/// <summary>
/// Motivo por el cual una solicitud de acceso fue rechazada.
/// </summary>
public enum MotivoRechazo
{
    Ninguno = 0,
    CredencialNoRegistrada = 1,
    EmpleadoInactivo = 2,
    CredencialNoActiva = 3,
    PuntoAccesoNoHabilitado = 4,
    SalidaSinEntradaPrevia = 5
}
