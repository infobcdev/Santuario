using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Santuario.Entidade.Entities;

namespace Santuario.Negocio.Mapeamentos
{
    public class NoticiaConfig : IEntityTypeConfiguration<Noticia>
    {
        public void Configure(EntityTypeBuilder<Noticia> e)
        {
            e.ToTable("noticia");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .UseIdentityByDefaultColumn();

            e.Property(x => x.Slug)
                .HasColumnName("slug")
                .HasMaxLength(250)
                .IsRequired();

            e.HasIndex(x => x.Slug)
                .IsUnique()
                .HasDatabaseName("ux_noticia_slug");

            e.Property(x => x.ImagemCapaUrl)
                .HasColumnName("imagemcapaurl")
                .HasMaxLength(500);

            e.Property(x => x.Titulo)
                .HasColumnName("titulo")
                .HasMaxLength(250)
                .IsRequired();

            e.Property(x => x.Categoria)
                .HasColumnName("categoria")
                .HasMaxLength(150)
                .IsRequired();

            e.Property(x => x.Subcategoria)
                .HasColumnName("subcategoria")
                .HasMaxLength(150);

            // resumo: escolha 1
            e.Property(x => x.Resumo)
                .HasColumnName("resumo")
                .HasMaxLength(600);

            // Conteúdo JSON como jsonb
            e.Property(x => x.ConteudoJson)
                .HasColumnName("conteudojson")
                .HasColumnType("jsonb")
                .IsRequired();

            //HTML pode ser grande => text
            e.Property(x => x.ConteudoHtml)
                .HasColumnName("conteudohtml")
                .HasColumnType("text");

            e.Property(x => x.PermiteComentarios)
                .HasColumnName("permitecomentarios")
                .HasDefaultValue(true);

            e.Property(x => x.Status)
                .HasColumnName("status")
                .HasDefaultValue(0);

            e.Property(x => x.DataPublicacao)
                .HasColumnName("datapublicacao");

            // Auditoria (base)
            e.Property(x => x.DataCriacao).HasColumnName("datacriacao");
            e.Property(x => x.DataAlteracao).HasColumnName("dataalteracao");
            e.Property(x => x.IdUsuarioCriacao).HasColumnName("idusuariocriacao");
            e.Property(x => x.IdUsuarioAlteracao).HasColumnName("idusuarioalteracao");

            // Relacionamento 1-N
            e.HasMany(x => x.Comentarios)
                .WithOne(x => x.Noticia)
                .HasForeignKey(x => x.IdNoticia)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices úteis para listagem pública/admin
            e.HasIndex(x => new { x.Status, x.DataPublicacao })
                .HasDatabaseName("ix_noticia_status_datapublicacao");

            e.HasIndex(x => new { x.Categoria, x.Status })
                .HasDatabaseName("ix_noticia_categoria_status");
        }
    }
}