using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Raíz de agregado: registro de un movimiento de acceso (RF-03).
/// Se registra tanto lo autorizado como lo rechazado, garantizando trazabilidad (RF-04).
/// Invariantes:
///  - Un registro rechazado siempre debe indicar un motivo distinto de Ninguno.
///  - Un registro autorizado no puede tener motivo de rechazo.
///  - El momento del dato no puede ser futuro.
/// </summary>
public sealed class RegistroAcceso : Entity
{
    public DateTime FechaHora { get; private set; }
    public Guid? EmpleadoId { get; private set; }
    public Guid? CredencialId { get; private set; }
    public string CodigoUtilizado { get; private set; } = default!;
    public Guid PuntoAccesoId { get; private set; }
    public TipoMovimiento TipoMovimiento { get; private set; }
    public ResultadoAcceso Resultado { get; private set; }
    public MotivoRechazo MotivoRechazo { get; private set; }

    private RegistroAcceso() { }

    private RegistroAcceso(DateTime fechaHora, Guid? empleadoId, Guid? credencialId,
        ValueObjects.CodigoCredencial codigoUtilizado, Guid puntoAccesoId,
        TipoMovimiento tipoMovimiento, ResultadoAcceso resultado, MotivoRechazo motivoRechazo)
        : base(Guid.NewGuid())
    {
        ValidarInvariantes(resultado, motivoRechazo, fechaHora);

        FechaHora = fechaHora;
        EmpleadoId = empleadoId;
        CredencialId = credencialId;
        CodigoUtilizado = codigoUtilizado.Valor;
        PuntoAccesoId = puntoAccesoId;
        TipoMovimiento = tipoMovimiento;
        Resultado = resultado;
        MotivoRechazo = motivoRechazo;
    }

    /// <summary>
    /// Registra un movimiento cuyo resultado ya fue determinado por el
    /// servicio de dominio <see cref="Services.ValidadorDeAcceso"/>.
    /// </summary>
    public static RegistroAcceso Registrar(
        DateTime fechaHora,
        Guid? empleadoId,
        Guid? credencialId,
        ValueObjects.CodigoCredencial codigoUtilizado,
        Guid puntoAccesoId,
        TipoMovimiento tipoMovimiento,
        Services.ResultadoValidacion resultado)
    {
        var (resultadoAcceso, motivo) = resultado.EsAutorizado
            ? (ResultadoAcceso.Autorizado, MotivoRechazo.Ninguno)
            : (ResultadoAcceso.Rechazado, resultado.Motivo);

        return new RegistroAcceso(
            fechaHora, empleadoId, credencialId,
            codigoUtilizado, puntoAccesoId, tipoMovimiento,
            resultadoAcceso, motivo);
    }

    private static void ValidarInvariantes(
        ResultadoAcceso resultado, MotivoRechazo motivo, DateTime fechaHora)
    {
        if (fechaHora > DateTime.UtcNow)
            throw new DomainException("La fecha y hora del registro no puede ser futura.");

        if (resultado == ResultadoAcceso.Rechazado && motivo == MotivoRechazo.Ninguno)
            throw new DomainException("Un acceso rechazado debe indicar el motivo del rechazo.");

        if (resultado == ResultadoAcceso.Autorizado && motivo != MotivoRechazo.Ninguno)
            throw new DomainException("Un acceso autorizado no puede tener motivo de rechazo.");
    }
}
