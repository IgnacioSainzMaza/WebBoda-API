using Microsoft.EntityFrameworkCore;
using WebBoda.Domain.Entities;

namespace WebBoda.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Invitado> Invitados => Set<Invitado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invitado>(entity =>
        {
            entity.ToTable("invitados");
            entity.HasKey(e => e.Id);

            // El token sigue siendo único e indexado: es el identificador del
            // enlace de modificación y se consulta en cada petición a ese endpoint.
            entity.HasIndex(e => e.TokenAcceso)
                  .IsUnique()
                  .HasDatabaseName("ix_invitados_token_acceso");

            entity.Property(e => e.TokenAcceso).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellidos).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CondicionesAlimentarias).HasMaxLength(500);
            entity.Property(e => e.Cancion).HasMaxLength(300);

            entity.Property(e => e.AutobusVuelta).HasConversion<int>();
        });
    }
}
