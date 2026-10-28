using SGCAE.Application.Common.Dtos;
using SGCAE.Domain.Entities;

namespace SGCAE.Application;

/// <summary>Extensiones de proyección de entidades de dominio hacia DTOs de lectura.</summary>
public static class Mapeos
{
    public static EmpleadoDto ADto(this Empleado empleado)
        => new(
            empleado.Id,
            empleado.Nombres,
            empleado.Apellidos,
            empleado.Email.Valor,
            empleado.Cargo,
            empleado.AreaOrganizacional,
            empleado.Estado,
            empleado.FechaContratacion,
            empleado.Credenciales.Count(c => c.Estado == Domain.Enums.EstadoCredencial.Activa));

    public static CredencialDto ADto(this Credencial credencial, string nombreEmpleado)
        => new(
            credencial.Id,
            credencial.EmpleadoId,
            nombreEmpleado,
            credencial.Codigo.Valor,
            credencial.Estado,
            credencial.FechaEmision);

    public static PuntoAccesoDto ADto(this PuntoAcceso punto)
        => new(punto.Id, punto.Nombre, punto.Descripcion, punto.Habilitado);
}
