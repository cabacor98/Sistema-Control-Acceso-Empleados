using SGCAE.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SGCAE.Infrastructure.Persistence;

/// <summary>
/// Datos semilla de puntos de acceso físicos de la sede principal (RF-06).
/// Se aplican vía migración para que la base quede lista para la demo.
/// </summary>
internal static class PuntosAccesoSemilla
{
    public static readonly Guid RecepcionId = new("0aa9c3d4-7006-4d61-9e30-1d3a5b171a01");
    public static readonly Guid ParqueaderoId = new("0aa9c3d4-7006-4d61-9e30-1d3a5b171a02");
    public static readonly Guid BodegaId = new("0aa9c3d4-7006-4d61-9e30-1d3a5b171a03");
    public static readonly Guid AreaRestringidaId = new("0aa9c3d4-7006-4d61-9e30-1d3a5b171a04");
    public static readonly Guid ZonaInhabilitadaId = new("0aa9c3d4-7006-4d61-9e30-1d3a5b171a05");

    public static void Aplicar(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<PuntoAcceso>().HasData(
            PuntoAcceso.Reconstruir(
                RecepcionId, "Recepción", "Ingreso principal de la sede (área general)", habilitado: true),
            PuntoAcceso.Reconstruir(
                ParqueaderoId, "Parqueadero", "Portería de acceso vehicular", habilitado: true),
            PuntoAcceso.Reconstruir(
                BodegaId, "Bodega", "Zona de almacenamiento y despacho", habilitado: true),
            PuntoAcceso.Reconstruir(
                AreaRestringidaId, "Área Restringida A", "Sala de servidores - acceso autorizado", habilitado: true),
            PuntoAcceso.Reconstruir(
                ZonaInhabilitadaId, "Almacén Temporal", "Zona en acondicionamiento", habilitado: false));
}
