using SGCAE.Application.Commands;
using SGCAE.Application.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Enums;
using SGCAE.Domain.Repositories;
using SGCAE.Domain.ValueObjects;
using MediatR;

namespace SGCAE.UnitTests.Application;

/// <summary>
/// Repositorios en memoria: demuestran que los handlers de la capa de
/// aplicación dependen solo de abstracciones (Clean Architecture).
/// </summary>
public class RegistrarSolicitudAccesoHandlerTests
{
    // ---------- Fixtures en memoria ----------

    private sealed class Ambiente
    {
        public Dictionary<Guid, Empleado> Empleados { get; } = [];
        public Dictionary<Guid, Credencial> Credenciales { get; } = [];
        public Dictionary<string, Guid> Codigos { get; } = [];
        public List<RegistroAcceso> Registros { get; } = [];
        public PuntoAcceso Punto { get; } = PuntoAcceso.Reconstruir(
            Guid.NewGuid(), "Recepción", "Ingreso principal", habilitado: true);

        public IEmpleadoRepository EmpleadoRepositorio { get; }
        public ICredencialRepository CredencialRepositorio { get; }
        public IPuntoAccesoRepository PuntoRepositorio { get; }
        public IRegistroAccesoRepository RegistroRepositorio { get; }
        public IUnitOfWork UnitOfWork { get; } = new UnitOfWorkVacia();

        public Ambiente()
        {
            EmpleadoRepositorio = new EmpleadoRepoEnMemoria(this);
            CredencialRepositorio = new CredencialRepoEnMemoria(this);
            PuntoRepositorio = new PuntoAccesoRepoEnMemoria(this);
            RegistroRepositorio = new RegistroRepoEnMemoria(this);
        }
    }

    private sealed class UnitOfWorkVacia : IUnitOfWork
    {
        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class EmpleadoRepoEnMemoria(Ambiente env) : IEmpleadoRepository
    {
        public Task<Empleado?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(env.Empleados.GetValueOrDefault(id));

        public Task<bool> ExisteEmailAsync(string email, Guid? idExcluido = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AgregarAsync(Empleado empleado, CancellationToken cancellationToken = default)
        {
            env.Empleados[empleado.Id] = empleado;
            return Task.CompletedTask;
        }
    }

    private sealed class CredencialRepoEnMemoria(Ambiente env) : ICredencialRepository
    {
        public Task<Credencial?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(env.Credenciales.GetValueOrDefault(id));

        public Task<Credencial?> ObtenerPorCodigoAsync(CodigoCredencial codigo, CancellationToken cancellationToken = default)
            => Task.FromResult(env.Codigos.TryGetValue(codigo.Valor, out var id)
                ? env.Credenciales.GetValueOrDefault(id)
                : null);

        public Task<bool> ExisteCodigoAsync(CodigoCredencial codigo, CancellationToken cancellationToken = default)
            => Task.FromResult(env.Codigos.ContainsKey(codigo.Valor));

        public Task<IReadOnlyList<Credencial>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<Credencial>)env.Credenciales.Values
                .Where(c => c.EmpleadoId == empleadoId).ToList().AsReadOnly());

        public Task AgregarAsync(Credencial credencial, CancellationToken cancellationToken = default)
        {
            env.Credenciales[credencial.Id] = credencial;
            env.Codigos[credencial.Codigo.Valor] = credencial.Id;
            return Task.CompletedTask;
        }
    }

    private sealed class PuntoAccesoRepoEnMemoria(Ambiente env) : IPuntoAccesoRepository
    {
        public Task<PuntoAcceso?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(env.Punto.Id == id ? env.Punto : null);

        public Task<IReadOnlyList<PuntoAcceso>> ListarAsync(bool? soloHabilitados = null, CancellationToken cancellationToken = default)
            => Task.FromResult((IReadOnlyList<PuntoAcceso>)[env.Punto]);

        public Task<bool> ExisteNombreAsync(string nombre, CancellationToken cancellationToken = default)
            => Task.FromResult(env.Punto.Nombre == nombre);

        public Task AgregarAsync(PuntoAcceso puntoAcceso, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class RegistroRepoEnMemoria(Ambiente env) : IRegistroAccesoRepository
    {
        public Task AgregarAsync(RegistroAcceso registro, CancellationToken cancellationToken = default)
        {
            env.Registros.Add(registro);
            return Task.CompletedTask;
        }

        public Task<bool> TieneEntradaAbiertaAsync(Guid empleadoId, CancellationToken cancellationToken = default)
        {
            var ultimo = env.Registros
                .Where(r => r.EmpleadoId == empleadoId && r.Resultado == ResultadoAcceso.Autorizado)
                .OrderByDescending(r => r.FechaHora)
                .Select(r => (TipoMovimiento?)r.TipoMovimiento)
                .FirstOrDefault();

            return Task.FromResult(ultimo == TipoMovimiento.Entrada);
        }
    }

    // ---------- Escenarios ----------

    private static Empleado RegistrarEmpleadoActivo()
        => Empleado.Registrar(
            "Andrés", "Méndez", "a.mendez@empresa.com", "Analista", "Logística", DateTime.UtcNow);

    private static Ambiente CrearAmbiente(bool empleadoActivo = true)
    {
        var env = new Ambiente();
        var empleado = RegistrarEmpleadoActivo();

        var credencial = empleado.AsignarCredencial(CodigoCredencial.Crear("CRED9009"));

        env.Empleados[empleado.Id] = empleado;
        env.Credenciales[credencial.Id] = credencial;
        env.Codigos[credencial.Codigo.Valor] = credencial.Id;

        if (!empleadoActivo)
            empleado.Desactivar(); // revoca automáticamente la credencial (RF-01)

        return env;
    }

    private static RegistrarSolicitudAccesoHandler CrearHandler(Ambiente env) => new(
        env.CredencialRepositorio, env.EmpleadoRepositorio, env.PuntoRepositorio,
        env.RegistroRepositorio, env.UnitOfWork, new ValidadorDeAcceso());

    private const string CodigoDelEmpleado = "CRED9009";

    [Fact]
    public async Task Entrada_ConCredencialActiva_SeAutorizaYRegistra()
    {
        var env = CrearAmbiente();

        var resultado = await CrearHandler(env).Handle(
            new RegistrarSolicitudAccesoCommand(CodigoDelEmpleado, env.Punto.Id, TipoMovimiento.Entrada),
            CancellationToken.None);

        Assert.True(resultado.Autorizado);
        Assert.Equal(MotivoRechazo.Ninguno, resultado.MotivoRechazo);
        Assert.Equal("Andrés Méndez", resultado.NombreEmpleado);
        Assert.Single(env.Registros);
    }

    [Fact]
    public async Task Entrada_EmpleadoInactivo_SeRechazaYSeRegistra()
    {
        var env = CrearAmbiente(empleadoActivo: false);

        var resultado = await CrearHandler(env).Handle(
            new RegistrarSolicitudAccesoCommand(CodigoDelEmpleado, env.Punto.Id, TipoMovimiento.Entrada),
            CancellationToken.None);

        Assert.False(resultado.Autorizado);
        Assert.Equal(MotivoRechazo.EmpleadoInactivo, resultado.MotivoRechazo);
        Assert.Single(env.Registros); // el rechazo queda trazado en el historial
    }

    [Fact]
    public async Task Salida_SinEntradaPrevia_SeRechaza()
    {
        var env = CrearAmbiente();

        var resultado = await CrearHandler(env).Handle(
            new RegistrarSolicitudAccesoCommand(CodigoDelEmpleado, env.Punto.Id, TipoMovimiento.Salida),
            CancellationToken.None);

        Assert.False(resultado.Autorizado);
        Assert.Equal(MotivoRechazo.SalidaSinEntradaPrevia, resultado.MotivoRechazo);
    }

    [Fact]
    public async Task EntradaSeguidaSalida_TraceCompletoDeDosMovimientos()
    {
        var env = CrearAmbiente();
        var handler = CrearHandler(env);

        await handler.Handle(new RegistrarSolicitudAccesoCommand(
            CodigoDelEmpleado, env.Punto.Id, TipoMovimiento.Entrada), CancellationToken.None);

        var salida = await handler.Handle(new RegistrarSolicitudAccesoCommand(
            CodigoDelEmpleado, env.Punto.Id, TipoMovimiento.Salida), CancellationToken.None);

        Assert.True(salida.Autorizado);
        Assert.Equal(2, env.Registros.Count);
    }
}
