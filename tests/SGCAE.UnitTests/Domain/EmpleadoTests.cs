using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Exceptions;

namespace SGCAE.UnitTests.Domain;

public class EmpleadoTests
{
    private static Empleado RegistrarActivo(string email = "j.perez@empresa.com") =>
        Empleado.Registrar("Juan", "Pérez Gómez", email, "Operario", "Operaciones", DateTime.UtcNow.AddDays(-100));

    [Fact]
    public void Registrar_ConDatosValidos_QuedaActivo()
    {
        var empleado = RegistrarActivo();

        Assert.Equal(EstadoLaboral.Activo, empleado.Estado);
        Assert.Equal("j.perez@empresa.com", empleado.Email.Valor);
        Assert.Empty(empleado.Credenciales);
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("@empresa.com")]
    [InlineData("juan@")]
    [InlineData("juan@site")]
    [InlineData("ju..an@empresa.com")]
    [InlineData("juan@empresa..com")]
    [InlineData("")]
    public void Registrar_EmailInvalido_LanzaExcepcion(string email)
    {
        Assert.Throws<DomainException>(() => Empleado.Registrar(
            "Juan", "Pérez Gómez", email, "Operario", "Operaciones", DateTime.UtcNow));
    }

    [Theory]
    [InlineData("", "Pérez", "Operario", "Operaciones")]
    [InlineData("Juan", "", "Operario", "Operaciones")]
    [InlineData("Juan", "Pérez", "", "Operaciones")]
    [InlineData("Juan", "Pérez", "Operario", "")]
    public void Registrar_DatosPersonalesVacios_LanzaExcepcion(
        string nombres, string apellidos, string cargo, string area)
    {
        Assert.Throws<DomainException>(() => Empleado.Registrar(
            nombres, apellidos, "ok@empresa.com", cargo, area, DateTime.UtcNow));
    }

    [Fact]
    public void AsignarCredencial_EmpleadoActivo_CredencialQuedaActiva()
    {
        var empleado = RegistrarActivo();

        var credencial = empleado.AsignarCredencial(
            CodigoCredencial.Crear("CRED9001"));

        Assert.Equal(EstadoCredencial.Activa, credencial.Estado);
        Assert.Equal(empleado.Id, credencial.EmpleadoId);
        Assert.Single(empleado.Credenciales);
    }

    [Fact]
    public void AsignarCredencial_CodigoRepetidoParaMismoEmpleado_LanzaExcepcion()
    {
        var empleado = RegistrarActivo();
        empleado.AsignarCredencial(CodigoCredencial.Crear("CRED9001"));

        Assert.Throws<DomainException>(() => empleado.AsignarCredencial(
            CodigoCredencial.Crear("CRED9001")));
    }

    [Fact]
    public void Desactivar_RevocaCredencialesActivas()
    {
        var empleado = RegistrarActivo();
        var credencial = empleado.AsignarCredencial(
            CodigoCredencial.Crear("CRED9002"));

        empleado.Desactivar();

        Assert.Equal(EstadoLaboral.Inactivo, empleado.Estado);
        Assert.Equal(EstadoCredencial.Revocada, credencial.Estado);
    }

    [Fact]
    public void Desactivar_EmpleadoYaInactivo_LanzaExcepcion()
    {
        var empleado = RegistrarActivo();
        empleado.Desactivar();

        Assert.Throws<DomainException>(() => empleado.Desactivar());
    }

    [Fact]
    public void AsignarCredencial_EmpleadoInactivo_LanzaExcepcion()
    {
        var empleado = RegistrarActivo();
        empleado.Desactivar();

        Assert.Throws<DomainException>(() => empleado.AsignarCredencial(
            CodigoCredencial.Crear("CRED9003")));
    }

    [Fact]
    public void ActualizarDatos_EntradaSensibleDeEmail_SeNormaliza()
    {
        var empleado = RegistrarActivo("juan.perez@empresa.com");
        empleado.ActualizarDatos("Juan", "Pérez Gómez", "JUAN.PEREZ@EMPRESA.COM", "Supervisor", "Operaciones");

        Assert.Equal("supervisor", empleado.Cargo.ToLowerInvariant());
        Assert.Equal("juan.perez@empresa.com", empleado.Email.Valor);
    }
}
