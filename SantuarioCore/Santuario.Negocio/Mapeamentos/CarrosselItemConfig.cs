using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Santuario.Entidade.Entities;

namespace Santuario.Negocio.Mapeamentos
{
    public class CarrosselItemConfig : IEntityTypeConfiguration<CarrosselItem>
    {
        public void Configure(EntityTypeBuilder<CarrosselItem> e)
        {
            e.ToTable("carrossel_item");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
             .HasColumnName("id")
             .ValueGeneratedOnAdd()
             .UseIdentityByDefaultColumn();

            e.Property(x => x.Ordem)
             .HasColumnName("ordem");

            e.Property(x => x.ImagemUrl)
             .HasColumnName("imagemurl")
             .HasMaxLength(500);

            e.Property(x => x.Titulo)
             .HasColumnName("titulo")
             .HasMaxLength(200);

            e.Property(x => x.Descricao)
             .HasColumnName("descricao");

            e.Property(x => x.Ativo)
             .HasColumnName("ativo");

            // ===============================
            // Auditoria (classe base)
            // ===============================

            e.Property(x => x.DataCriacao)
             .HasColumnName("datacriacao");

            e.Property(x => x.DataAlteracao)
             .HasColumnName("dataalteracao");

            e.Property(x => x.IdUsuarioCriacao)
             .HasColumnName("idusuariocriacao");

            e.Property(x => x.IdUsuarioAlteracao)
             .HasColumnName("idusuarioalteracao");
        }
    }
}