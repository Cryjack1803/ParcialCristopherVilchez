using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ParcialVilchezCristopher_.Models;

namespace ParcialVilchezCristopher_.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_Clientes_IngresosMensuales_Positive", "IngresosMensuales > 0"));
            entity.Property(c => c.IngresosMensuales).HasPrecision(18, 2);
            entity.HasIndex(c => c.UsuarioId).IsUnique();
            entity.HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SolicitudCredito>(entity =>
        {
            entity.Property(s => s.MontoSolicitado).HasPrecision(18, 2);
            entity.Property(s => s.Estado).HasConversion<int>();
            entity.ToTable(t => t.HasCheckConstraint("CK_SolicitudesCredito_MontoSolicitado_Positive", "MontoSolicitado > 0"));
            entity.HasOne(s => s.Cliente)
                .WithMany(c => c.SolicitudesCredito)
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(s => s.ClienteId)
                .HasDatabaseName("IX_SolicitudesCredito_ClienteId_Pendiente")
                .IsUnique()
                .HasFilter("Estado = 0");
        });
    }
}
