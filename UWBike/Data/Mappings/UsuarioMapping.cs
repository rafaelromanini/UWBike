using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UWBike.Model;

namespace UWBike.Data.Mappings
{
    public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("TB_USUARIO", "RM554637");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .HasColumnName("ID_USUARIO");

            builder.Property(u => u.Nome)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("NOME");

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("EMAIL");

            builder.Property(u => u.Senha)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("SENHA");

            builder.Property(u => u.DataCriacao)
                .IsRequired()
                .HasColumnName("DATA_CRIACAO");

            builder.Property(u => u.DataAtualizacao)
                .HasColumnName("DATA_ATUALIZACAO");

            // Índice único para email
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_USUARIO_EMAIL");
        }
    }
}