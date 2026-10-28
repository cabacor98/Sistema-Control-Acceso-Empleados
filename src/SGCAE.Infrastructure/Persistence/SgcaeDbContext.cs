using SGCAE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SGCAE.Infrastructure.Persistence.Configurations;

namespace SGCAE.Infrastructure.Persistence;

/// <summary>
/// Contexto de datos de EF Core sobre SQL Server.
/// Satisface el contexto de solo lectura (ISgcaeReadRepository) para las
/// consultas CQRS y la unidad de trabajo (IUnitOfWork) para los comandos.
/// </summary>
public sealed class SgcaeDbContext(DbContextOptions<SgcaeDbContext> options) : DbContext(options)
{
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Credencial> Credenciales => Set<Credencial>();
    public DbSet<PuntoAcceso> PuntosAcceso => Set<PuntoAcceso>();
    public DbSet<RegistroAcceso> RegistrosAcceso => Set<RegistroAcceso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SgcaeDbContext).Assembly);

        PuntosAccesoSemilla.Aplicar(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }
}
