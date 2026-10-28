using SGCAE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SGCAE.Infrastructure.Persistence.Configurations;

internal sealed class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleados");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nombres).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Apellidos).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Cargo).HasMaxLength(100).IsRequired();
        builder.Property(e => e.AreaOrganizacional).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Estado).HasConversion<int>().IsRequired();

        // Objeto de valor como columna propietaria en la misma tabla.
        builder.OwnsOne(e => e.Email, email =>
        {
            email.Property(v => v.Valor)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            // Invariante: el correo identifica de forma única al empleado.
            email.HasIndex(v => v.Valor).IsUnique().HasDatabaseName("UX_Empleados_Email");
        });

        builder.Navigation(e => e.Credenciales).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(e => e.Credenciales)
            .WithOne()
            .HasForeignKey(c => c.EmpleadoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CredencialConfiguration : IEntityTypeConfiguration<Credencial>
{
    public void Configure(EntityTypeBuilder<Credencial> builder)
    {
        builder.ToTable("Credenciales");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.EmpleadoId).IsRequired();
        builder.Property(c => c.Estado).HasConversion<int>().IsRequired();
        builder.Property(c => c.FechaEmision).IsRequired();

        builder.OwnsOne(c => c.Codigo, codigo =>
        {
            codigo.Property(v => v.Valor)
                .HasColumnName("Codigo")
                .HasMaxLength(20)
                .IsRequired();

            // Invariante: el código de credencial es único en toda la organización.
            codigo.HasIndex(v => v.Valor).IsUnique().HasDatabaseName("UX_Credenciales_Codigo");
        });
    }
}

internal sealed class PuntoAccesoConfiguration : IEntityTypeConfiguration<PuntoAcceso>
{
    public void Configure(EntityTypeBuilder<PuntoAcceso> builder)
    {
        builder.ToTable("PuntosAcceso");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Descripcion).HasMaxLength(250).IsRequired();
        builder.Property(p => p.Habilitado).IsRequired();

        builder.HasIndex(p => p.Nombre).IsUnique();
    }
}

internal sealed class RegistroAccesoConfiguration : IEntityTypeConfiguration<RegistroAcceso>
{
    public void Configure(EntityTypeBuilder<RegistroAcceso> builder)
    {
        builder.ToTable("RegistrosAcceso");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FechaHora).IsRequired();
        builder.Property(r => r.EmpleadoId);
        builder.Property(r => r.CredencialId);
        builder.Property(r => r.CodigoUtilizado).HasMaxLength(20).IsRequired();
        builder.Property(r => r.TipoMovimiento).HasConversion<int>().IsRequired();
        builder.Property(r => r.Resultado).HasConversion<int>().IsRequired();
        builder.Property(r => r.MotivoRechazo).HasConversion<int>().IsRequired();

        // Índices alineados con los filtros del historial (RF-04).
        builder.HasIndex(r => new { r.EmpleadoId, r.FechaHora });
        builder.HasIndex(r => new { r.PuntoAccesoId, r.FechaHora });
        builder.HasIndex(r => r.FechaHora);

        // NO ACTION: un empleado desactivado no se elimina; conserva los registros trazables.
        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(r => r.EmpleadoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Credencial>()
            .WithMany()
            .HasForeignKey(r => r.CredencialId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<PuntoAcceso>()
            .WithMany()
            .HasForeignKey(r => r.PuntoAccesoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
