using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Santuario.Entidade.Entities;
using System;
using System.Reflection;

namespace Santuario.Negocio.Context
{
    public class SantuarioDbContext : DbContext
    {
        public SantuarioDbContext(DbContextOptions<SantuarioDbContext> options)
            : base(options)
        {
        }

        // ============================
        // DbSets
        // ============================
        public DbSet<CarrosselItem> CarrosselItens { get; set; } = default!;
        public DbSet<Sobre> Sobre { get; set; } = default!;
        public DbSet<SobreTopico> SobreTopicos { get; set; } = default!;
        public DbSet<Noticia> Noticias { get; set; } = default!;
        public DbSet<NoticiaComentario> NoticiaComentarios { get; set; } = default!;
        public DbSet<Usuario> Usuarios { get; set; } = default!;

        // ============================
        // Conversores DateTime → UTC
        // ============================

        static DateTime ToUtc(DateTime v)
        {
            if (v.Kind == DateTimeKind.Utc) return v;
            if (v.Kind == DateTimeKind.Local) return v.ToUniversalTime();
            return DateTime.SpecifyKind(v, DateTimeKind.Local).ToUniversalTime();
        }

        static DateTime? ToUtcNullable(DateTime? v)
            => v.HasValue ? ToUtc(v.Value) : null;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Conversor global para UTC
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => ToUtc(v),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new ValueConverter<DateTime?, DateTime?>(
                            v => ToUtcNullable(v),
                            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
                        ));
                    }
                }
            }
        }
    }
}