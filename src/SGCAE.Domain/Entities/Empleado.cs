using SGCAE.Domain.Common;
using SGCAE.Domain.Enums;
using SGCAE.Domain.ValueObjects;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Raíz de agregado: empleado de la organización (RF-01).
/// Invariantes:
///  - Posée identificación única, nombres, apellidos, correo, cargo, área y estado laboral.
///  - Un empleado solo puede desactivarse cuando su vínculo laboral está activo.
///  - Un empleado activo no puede tener más de una credencial (no vigente) activa.
/// </summary>
public sealed class Empleado : Entity
{
    private readonly List<Credencial> _credenciales = [];

    public string Nombres { get; private set; } = default!;
    public string Apellidos { get; private set; } = default!;
    public CorreoElectronico Email { get; private set; } = default!;
    public string Cargo { get; private set; } = default!;
    public string AreaOrganizacional { get; private set; } = default!;
    public EstadoLaboral Estado { get; private set; }
    public DateTime FechaContratacion { get; private set; }

    public IReadOnlyCollection<Credencial> Credenciales => _credenciales.AsReadOnly();

    private Empleado() { }

    private Empleado(string nombres, string apellidos, CorreoElectronico email, string cargo,
        string areaOrganizacional, EstadoLaboral estado, DateTime fechaContratacion)
        : base(Guid.NewGuid())
    {
        EstablecerDatos(nombres, apellidos, cargo, areaOrganizacional);
        Email = email;
        Estado = estado;
        FechaContratacion = fechaContratacion;
    }
    public static Empleado Registrar(string nombres, string apellidos, string email, string cargo,
        string areaOrganizacional, DateTime fechaContratacion)
    {
        return new Empleado(nombres, apellidos, CorreoElectronico.Crear(email), cargo,
            areaOrganizacional, EstadoLaboral.Activo, fechaContratacion.Date);
    }

    /// <summary>
    /// Actualiza los datos personales y laborales (RF-01).
    /// </summary>
    public void ActualizarDatos(string nombres, string apellidos, string email, string cargo, string areaOrganizacional)
    {
        EstablecerDatos(nombres, apellidos, cargo, areaOrganizacional);
        Email = CorreoElectronico.Crear(email);
    }

    /// <summary>
    /// Invalida el vínculo laboral (RF-01). Si mantiene una credencial activa,
    /// esta queda automáticamente revocada para impedir su uso futuro.
    /// </summary>
    public void Desactivar()
    {
        if (Estado == EstadoLaboral.Inactivo)
            throw new DomainException($"El empleado {Id} ya se encuentra inactivo.");

        Estado = EstadoLaboral.Inactivo;

        foreach (var credencial in _credenciales.Where(c => c.Estado == EstadoCredencial.Activa))
        {
            credencial.Revocar();
        }
    }

    /// <summary>
    /// Asigna una nueva credencial de acceso (RF-02).
    /// Solo empleados con vínculo activo pueden recibir credenciales.
    /// </summary>
    public Credencial AsignarCredencial(CodigoCredencial codigo)
    {
        if (Estado == EstadoLaboral.Inactivo)
            throw new DomainException("No se puede asignar una credencial a un empleado con vínculo laboral inactivo.");

        if (_credenciales.Any(c => c.Codigo == codigo))
            throw new DomainException($"El empleado ya posee la credencial con código {codigo.Valor}.");

        var credencial = new Credencial(Id, codigo);
        _credenciales.Add(credencial);
        return credencial;
    }

    private void EstablecerDatos(string nombres, string apellidos, string cargo, string areaOrganizacional)
    {
        if (string.IsNullOrWhiteSpace(nombres))
            throw new DomainException("Los nombres son obligatorios.");
        if (string.IsNullOrWhiteSpace(apellidos))
            throw new DomainException("Los apellidos son obligatorios.");
        if (string.IsNullOrWhiteSpace(cargo))
            throw new DomainException("El cargo es obligatorio.");
        if (string.IsNullOrWhiteSpace(areaOrganizacional))
            throw new DomainException("El área organizacional es obligatoria.");

        if (nombres.Length > 100) throw new DomainException("Los nombres no deben exceder 100 caracteres.");
        if (apellidos.Length > 100) throw new DomainException("Los apellidos no deben exceder 100 caracteres.");
        if (cargo.Length > 100) throw new DomainException("El cargo no debe exceder 100 caracteres.");
        if (areaOrganizacional.Length > 100) throw new DomainException("El área organizacional no debe exceder 100 caracteres.");

        Nombres = nombres.Trim();
        Apellidos = apellidos.Trim();
        Cargo = cargo.Trim();
        AreaOrganizacional = areaOrganizacional.Trim();
    }
}
