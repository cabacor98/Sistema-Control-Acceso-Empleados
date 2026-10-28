using SGCAE.Domain.ValueObjects;

namespace SGCAE.UnitTests.Domain;

public class CodigoCredencialTests
{
    [Fact]
    public void Crear_CodigoConGuion_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(() => CodigoCredencial.Crear("cred-2025"));
    }

    [Theory]
    [InlineData("CRED2025")]
    [InlineData("cred2025")]
    [InlineData("A1B2C3D4E5")]
    public void Crear_CodigoValido_SeNormalizaYAcepta(string texto)
    {
        var codigo = CodigoCredencial.Crear(texto);

        Assert.Equal(texto.Trim().ToUpperInvariant(), codigo.Valor);
    }

    [Fact]
    public void Crear_DosCodigosDiferentesPorCaso_AsimismoValor_SonIguales()
    {
        var primero = CodigoCredencial.Crear("cred112");
        var segundo = CodigoCredencial.Crear("CRED112");

        Assert.Equal(primero, segundo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_Vacio_LanzaExcepcion(string? texto)
    {
        Assert.Throws<DomainException>(() => CodigoCredencial.Crear(texto));
    }

    [Theory]
    [InlineData("ABC12")]        // menos de 6
    [InlineData("LETRAS1234567890123456FF")] // más de 20
    [InlineData("CRED 2025")]    // espacios
    [InlineData("CRED-2025")]    // guion
    [InlineData("CRED.2025")]    // punto
    public void Crear_FormatoInvalido_LanzaExcepcion(string texto)
    {
        Assert.Throws<DomainException>(() => CodigoCredencial.Crear(texto));
    }

    [Fact]
    public void TryCrear_CodigoValido_RetornaCodigo()
    {
        var genero = CodigoCredencial.TryCrear("cred4412", out var codigo);

        Assert.True(genero);
        Assert.NotNull(codigo);
        Assert.Equal("CRED4412", codigo!.Valor);
    }
}
