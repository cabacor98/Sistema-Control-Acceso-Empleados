using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Entidad de credencial de acceso (RF-02), perteneciente al agregado de Empleado.
/// Invariantes:
///  - Código único entre todas las credenciales (duplicidad protegida en persistencia).
///  - Solo las credenciales en estado Activa pueden usarse para validar accesos.
///  - Una credencial bloqueada puede reactivarse; una revocada no puede reactivarse.
/// </summary>
public sealed class Credencial : Entity
{
    public Guid EmpleadoId { get; private set; }
    public CodigoCredencial Codigo { get; private set; } = default!;
    public EstadoCredencial Estado { get; private set; }
    public DateTime FechaEmision { get; private set; }

    private Credencial() { }

    internal Credencial(Guid empleadoId, CodigoCredencial codigo)
        : base(Guid.NewGuid())
    {
        EmpleadoId = empleadoId;
        Codigo = codigo;
        Estado = EstadoCredencial.Activa;
        FechaEmision = DateTime.UtcNow;
    }

    public bool EsUtilizable(DateTime momentoActual)
        => Estado == EstadoCredencial.Activa;

    /// <summary>Suspende temporalmente una credencial activa.</summary>
    public void Bloquear()
    {
        if (Estado == EstadoCredencial.Revocada)
            throw new DomainException("No se puede bloquear una credencial revocada.");

        if (Estado == EstadoCredencial.Bloqueada)
            throw new DomainException("La credencial ya se encuentra bloqueada.");

        Estado = EstadoCredencial.Bloqueada;
    }

    /// <summary>Reactiva una credencial bloqueada.</summary>
    public void Reactivar()
    {
        if (Estado != EstadoCredencial.Bloqueada)
            throw new DomainException("Solo se puede reactivar una credencial bloqueada.");

        Estado = EstadoCredencial.Activa;
    }

    /// <summary>Cancela de forma definitiva la credencial.</summary>
    public void Revocar()
    {
        if (Estado == EstadoCredencial.Revocada)
            throw new DomainException("La credencial ya se encuentra revocada.");

        Estado = EstadoCredencial.Revocada;
    }
}
