namespace SGCAE.Domain.Repositories;

/// <summary>
/// Unidad de trabajo: agrupa las escrituras de todos los agregados en una
/// única transacción coherente.
/// </summary>
public interface IUnitOfWork
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
