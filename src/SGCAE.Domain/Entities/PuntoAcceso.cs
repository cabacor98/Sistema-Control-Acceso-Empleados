using SGCAE.Domain.Common;

namespace SGCAE.Domain.Entities;

/// <summary>
/// Entidad: punto de acceso físico (RF-06): recepción, parqueadero, bodega, áreas restringidas, etc.
/// Invariante: nombre único y descripción breve no vacía.
/// </summary>
public sealed class PuntoAcceso : Entity
{
    public string Nombre { get; private set; } = default!;
    public string Descripcion { get; private set; } = default!;
    public bool Habilitado { get; private set; }

    private PuntoAcceso() { }

    private PuntoAcceso(Guid id, string nombre, string descripcion, bool habilitado)
        : base(id)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        Habilitado = habilitado;
    }

    public static PuntoAcceso Registrar(string nombre, string descripcion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del punto de acceso es obligatorio.");

        if (nombre.Length > 100)
            throw new DomainException("El nombre del punto de acceso no debe exceder 100 caracteres.");

        if (string.IsNullOrWhiteSpace(descripcion))
            throw new DomainException("La descripción del punto de acceso es obligatoria.");

        if (descripcion.Length > 250)
            throw new DomainException("La descripción del punto de acceso no debe exceder 250 caracteres.");

        return new PuntoAcceso(Guid.NewGuid(), nombre.Trim(), descripcion.Trim(), habilitado: true);
    }

    /// <summary>Reconstruye un punto de acceso proveniente de la capa de persistencia.</summary>
    public static PuntoAcceso Reconstruir(Guid id, string nombre, string descripcion, bool habilitado)
        => new(id, nombre, descripcion, habilitado);

    public void Habilitar() => Habilitado = true;

    public void Inhabilitar() => Habilitado = false;
}
