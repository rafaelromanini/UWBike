using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UWBike.Model;

namespace UWBike.Data.Mappings
{
    public class PatioMapping : IEntityTypeConfiguration<Patio>
    {
        public void Configure(EntityTypeBuilder<Patio> builder)
        {
            builder.ToTable("TB_PATIO", "RM554637");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("ID_PATIO");

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("NOME");

            builder.Property(p => p.Endereco)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("ENDERECO");

            builder.Property(p => p.Cep)
                .HasMaxLength(10)
                .HasColumnName("CEP");

            builder.Property(p => p.Cidade)
                .HasMaxLength(50)
                .HasColumnName("CIDADE");

            builder.Property(p => p.Estado)
                .HasMaxLength(2)
                .HasColumnName("ESTADO");

            builder.Property(p => p.Telefone)
                .HasMaxLength(15)
                .HasColumnName("TELEFONE");

            builder.Property(p => p.Capacidade)
                .IsRequired()
                .HasColumnName("CAPACIDADE");

            builder.Property(p => p.Ativo)
                .IsRequired()
                .HasColumnName("ATIVO");

            builder.Property(p => p.DataCriacao)
                .IsRequired()
                .HasColumnName("DATA_CRIACAO");

            builder.Property(p => p.DataAtualizacao)
                .HasColumnName("DATA_ATUALIZACAO");

            // Relacionamento com Motos
            builder.HasMany(p => p.Motos)
                .WithOne(m => m.Patio)
                .HasForeignKey(m => m.PatioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}