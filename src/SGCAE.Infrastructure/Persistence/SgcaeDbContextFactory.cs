using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SGCAE.Infrastructure.Persistence;

/// <summary>
/// Fábrica de tiempo de diseño usada por `dotnet ef migrations add`.
/// Lee la cadena de conexión de appsettings.json del proyecto de Api
/// (con reemplazo por variable de entorno SGCAE_DB__CONNECTIONSTRING).
/// </summary>
internal sealed class SgcaeDbContextFactory : IDesignTimeDbContextFactory<SgcaeDbContext>
{
    public SgcaeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine("..", "SGCAE.Api", "appsettings.json"), optional: true)
            .AddEnvironmentVariables(prefix: "SGCAE_DB__")
            .Build();

        var cadena = configuration.GetConnectionString("SgcaeDb")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=SGCAE;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        return new SgcaeDbContext(new DbContextOptionsBuilder<SgcaeDbContext>()
            .UseSqlServer(cadena, sql => sql.MigrationsAssembly(typeof(SgcaeDbContext).Assembly))
            .Options);
    }
}
