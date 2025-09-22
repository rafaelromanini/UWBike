using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UWBike.Model;

namespace UWBike.Data.Mappings
{
    public class MotoMapping : IEntityTypeConfiguration<Moto>
    {
        public void Configure(EntityTypeBuilder<Moto> builder)
        {
            builder.ToTable("TB_MOTO", "RM554637"); // Tabela e schema no Oracle

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .HasColumnName("ID_MOTO");

            builder.Property(m => m.Modelo)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("MODELO");

            builder.Property(m => m.Placa)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("PLACA");

            builder.Property(m => m.Chassi)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("CHASSI");

            builder.Property(m => m.AnoFabricacao)
                .HasColumnName("ANO_FABRICACAO");

            builder.Property(m => m.Cor)
                .HasMaxLength(50)
                .HasColumnName("COR");

            builder.Property(m => m.Ativo)
                .IsRequired()
                .HasColumnName("ATIVO")
                .HasConversion<int>();

            builder.Property(m => m.DataCriacao)
                .IsRequired()
                .HasColumnName("DATA_CRIACAO");

            builder.Property(m => m.DataAtualizacao)
                .HasColumnName("DATA_ATUALIZACAO");

            builder.Property(m => m.PatioId)
                .IsRequired()
                .HasColumnName("ID_PATIO");

            // Relacionamento com Patio
            builder.HasOne(m => m.Patio)
                .WithMany(p => p.Motos)
                .HasForeignKey(m => m.PatioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices únicos para identificação
            builder.HasIndex(m => m.Placa)
                .IsUnique()
                .HasDatabaseName("IX_MOTO_PLACA");

            builder.HasIndex(m => m.Chassi)
                .IsUnique()
                .HasDatabaseName("IX_MOTO_CHASSI");
        }
    }
}
