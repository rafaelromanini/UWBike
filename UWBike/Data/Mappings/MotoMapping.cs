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
        }
    }
}
