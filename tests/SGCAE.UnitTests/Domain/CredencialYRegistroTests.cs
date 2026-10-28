using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Exceptions;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.UnitTests.Domain;

public class CredencialTests
{
    private static Credencial Nueva()
    {
        var empleado = Empleado.Registrar(
            "Ana", "López", "a.lopez@empresa.com", "Operaria", "Operaciones", DateTime.UtcNow);
        return empleado.AsignarCredencial(CodigoCredencial.Crear("CRED5551"));
    }

    [Fact]
    public void CredencialNueva_EstaActiva()
    {
        Assert.Equal(EstadoCredencial.Activa, Nueva().Estado);
    }

    [Fact]
    public void Bloquear_Y_Reactivar()
    {
        var credencial = Nueva();

        credencial.Bloquear();
        Assert.Equal(EstadoCredencial.Bloqueada, credencial.Estado);
        Assert.False(credencial.EsUtilizable(DateTime.UtcNow));

        credencial.Reactivar();
        Assert.Equal(EstadoCredencial.Activa, credencial.Estado);
    }

    [Fact]
    public void Bloquear_CredencialYaBloqueada_LanzaExcepcion()
    {
        var credencial = Nueva();
        credencial.Bloquear();

        Assert.Throws<DomainException>(() => credencial.Bloquear());
    }

    [Fact]
    public void Bloquear_CredencialRevocada_LanzaExcepcion()
    {
        var credencial = Nueva();
        credencial.Revocar();

        Assert.Throws<DomainException>(() => credencial.Bloquear());
    }

    [Fact]
    public void Revocar_EsDefinitivo()
    {
        var credencial = Nueva();
        credencial.Revocar();

        Assert.Equal(EstadoCredencial.Revocada, credencial.Estado);
        Assert.False(credencial.EsUtilizable(DateTime.UtcNow));
        Assert.Throws<DomainException>(() => credencial.Revocar());
        Assert.Throws<DomainException>(() => credencial.Reactivar());
    }
}

public class RegistroAccesoTests
{
    private static readonly Guid PuntoRecepcion = Guid.NewGuid();
    private static readonly Guid EmpleadoId = Guid.NewGuid();
    private static readonly Guid CredencialId = Guid.NewGuid();

    [Fact]
    public void Registrar_Autorizado_YaRechazado_AplicaInvariantes()
    {
        var autorizado = RegistroAcceso.Registrar(
            DateTime.UtcNow, EmpleadoId, CredencialId,
            CodigoCredencial.Crear("CRED7788"), PuntoRecepcion,
            TipoMovimiento.Entrada,
            new ResultadoValidacion(true, MotivoRechazo.Ninguno, EmpleadoId, CredencialId));

        var rechazado = RegistroAcceso.Registrar(
            DateTime.UtcNow, null, null,
            CodigoCredencial.Crear("CRED7788"), PuntoRecepcion,
            TipoMovimiento.Entrada,
            ResultadoValidacion.Rechazado(MotivoRechazo.EmpleadoInactivo));

        Assert.Equal(ResultadoAcceso.Autorizado, autorizado.Resultado);
        Assert.Equal(MotivoRechazo.Ninguno, autorizado.MotivoRechazo);
        Assert.Equal(ResultadoAcceso.Rechazado, rechazado.Resultado);
        Assert.Equal(MotivoRechazo.EmpleadoInactivo, rechazado.MotivoRechazo);
        Assert.Null(rechazado.EmpleadoId);
    }

    [Fact]
    public void Registrar_FechaFutura_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(() => RegistroAcceso.Registrar(
            DateTime.UtcNow.AddMinutes(5), EmpleadoId, CredencialId,
            CodigoCredencial.Crear("CRED7788"), PuntoRecepcion,
            TipoMovimiento.Entrada,
            new ResultadoValidacion(true, MotivoRechazo.Ninguno, EmpleadoId, CredencialId)));
    }

    [Fact]
    public void Registrar_RechazadoSinMotivo_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(() => RegistroAcceso.Registrar(
            DateTime.UtcNow, null, null,
            CodigoCredencial.Crear("CRED7788"), PuntoRecepcion,
            TipoMovimiento.Salida,
            ResultadoValidacion.Rechazado(MotivoRechazo.Ninguno)));
    }
}
