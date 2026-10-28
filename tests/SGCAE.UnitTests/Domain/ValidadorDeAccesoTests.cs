using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;

namespace SGCAE.UnitTests.Domain;

public class ValidadorDeAccesoTests
{
    private static readonly Guid PuntoId = Guid.NewGuid();

    private static readonly ValidadorDeAcceso Validador = new();

    /// <summary>
    /// Escenario coherente: un empleado con su credencial y un punto de acceso.
    /// El código utilizado se toma de la credencial real del escenario.
    /// </summary>
    private sealed record EscenarioData(Empleado Empleado, Credencial Credencial, PuntoAcceso Punto);

    private static EscenarioData CrearEscenario(
        bool empleadoActivo = true, EstadoCredencial estadoCredencial = EstadoCredencial.Activa,
        bool puntoHabilitado = true)
    {
        var empleado = Empleado.Registrar(
            "Ana", "López", "a.lopez@empresa.com", "Operaria", "Operaciones", DateTime.UtcNow);
        var credencial = empleado.AsignarCredencial(CodigoCredencial.Crear("CRED1122"));

        switch (estadoCredencial)
        {
            case EstadoCredencial.Bloqueada: credencial.Bloquear(); break;
            case EstadoCredencial.Revocada: credencial.Revocar(); break;
        }

        if (!empleadoActivo)
            empleado.Desactivar();

        var punto = PuntoAcceso.Reconstruir(
            PuntoId, "Recepción", "Ingreso principal", puntoHabilitado);

        return new(empleado, credencial, punto);
    }

    [Fact]
    public void Acceso_CondicionesCorrectas_SeAutoriza()
    {
        var escenario = CrearEscenario();

        var resultado = Validador.Validar(
            escenario.Empleado, escenario.Credencial, escenario.Credencial.Codigo,
            escenario.Punto, TipoMovimiento.Entrada, tieneEntradaAbierta: false, DateTime.UtcNow);

        Assert.True(resultado.EsAutorizado);
        Assert.Equal(MotivoRechazo.Ninguno, resultado.Motivo);
        Assert.Equal(escenario.Empleado.Id, resultado.EmpleadoId);
        Assert.Equal(escenario.Credencial.Id, resultado.CredencialId);
    }

    [Fact]
    public void Acceso_CredencialInexistente_SeRechaza()
    {
        var escenario = CrearEscenario();

        var resultado = Validador.Validar(
            null, null, CodigoCredencial.Crear("SINEXIST"),
            escenario.Punto, TipoMovimiento.Entrada, false, DateTime.UtcNow);

        Assert.False(resultado.EsAutorizado);
        Assert.Equal(MotivoRechazo.CredencialNoRegistrada, resultado.Motivo);
    }

    [Fact]
    public void Acceso_EmpleadoInactivo_SeRechaza()
    {
        var escenario = CrearEscenario(empleadoActivo: false);

        var resultado = Validador.Validar(
            escenario.Empleado, escenario.Credencial, escenario.Credencial.Codigo,
            escenario.Punto, TipoMovimiento.Entrada, false, DateTime.UtcNow);

        Assert.False(resultado.EsAutorizado);
        Assert.Equal(MotivoRechazo.EmpleadoInactivo, resultado.Motivo);
    }

    [Theory]
    [InlineData(EstadoCredencial.Bloqueada)]
    [InlineData(EstadoCredencial.Revocada)]
    public void Acceso_CredencialNoActiva_SeRechaza(EstadoCredencial estado)
    {
        var escenario = CrearEscenario(estadoCredencial: estado);

        var resultado = Validador.Validar(
            escenario.Empleado, escenario.Credencial, escenario.Credencial.Codigo,
            escenario.Punto, TipoMovimiento.Entrada, false, DateTime.UtcNow);

        Assert.False(resultado.EsAutorizado);
        Assert.Equal(MotivoRechazo.CredencialNoActiva, resultado.Motivo);
    }

    [Fact]
    public void Acceso_PuntoInhabilitado_SeRechaza()
    {
        var escenario = CrearEscenario(puntoHabilitado: false);

        var resultado = Validador.Validar(
            escenario.Empleado, escenario.Credencial, escenario.Credencial.Codigo,
            escenario.Punto, TipoMovimiento.Entrada, false, DateTime.UtcNow);

        Assert.False(resultado.EsAutorizado);
        Assert.Equal(MotivoRechazo.PuntoAccesoNoHabilitado, resultado.Motivo);
    }

    [Fact]
    public void Salida_SinEntradaPrevia_SeRechaza()
    {
        var escenario = CrearEscenario();

        var resultado = Validador.Validar(
            escenario.Empleado, escenario.Credencial, escenario.Credencial.Codigo,
            escenario.Punto, TipoMovimiento.Salida, tieneEntradaAbierta: false, DateTime.UtcNow);

        Assert.False(resultado.EsAutorizado);
        Assert.Equal(MotivoRechazo.SalidaSinEntradaPrevia, resultado.Motivo);
    }

    [Fact]
    public void Salida_ConEntradaPrevia_SeAutoriza()
    {
        var escenario = CrearEscenario();

        var resultado = Validador.Validar(
            escenario.Empleado, escenario.Credencial, escenario.Credencial.Codigo,
            escenario.Punto, TipoMovimiento.Salida, tieneEntradaAbierta: true, DateTime.UtcNow);

        Assert.True(resultado.EsAutorizado);
    }

    [Fact]
    public void Entrada_CredencialDeOtroEmpleado_SeRechaza()
    {
        // El empleado y la credencial no están vinculados entre sí.
        var escenario = CrearEscenario();
        var otraPersona = OtraPersona();

        var resultado = Validador.Validar(
            otraPersona, escenario.Credencial, escenario.Credencial.Codigo,
            escenario.Punto, TipoMovimiento.Entrada, false, DateTime.UtcNow);

        Assert.False(resultado.EsAutorizado);
        Assert.Equal(MotivoRechazo.CredencialNoRegistrada, resultado.Motivo);
    }

    private static Empleado OtraPersona() =>
        Empleado.Registrar("Beatriz", "Sánchez", "b.sanchez@empresa.com", "Gerente", "Planeación", DateTime.UtcNow);
}
