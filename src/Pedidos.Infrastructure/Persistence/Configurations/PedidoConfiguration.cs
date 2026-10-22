using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración de la tabla <c>Pedidos</c> (RF-06).
/// </summary>
public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.NumeroPedido)
            .HasMaxLength(Pedido.NumeroPedidoMaxLength)
            .IsRequired();

        // La unicidad se refuerza en la base de datos; la capa de aplicación solo avisa.
        builder.HasIndex(p => p.NumeroPedido)
            .IsUnique()
            .HasDatabaseName("IX_Pedidos_NumeroPedido");

        builder.Property(p => p.Cliente)
            .HasMaxLength(Pedido.ClienteMaxLength)
            .IsRequired();

        builder.Property(p => p.Fecha)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(p => p.Total)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.Estado)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.EsEliminado)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(p => p.FechaEliminacion)
            .HasColumnType("datetime2");

        // Eliminación lógica: los pedidos marcados como eliminados no aparecen en las consultas.
        builder.HasQueryFilter(p => !p.EsEliminado);
    }
}
