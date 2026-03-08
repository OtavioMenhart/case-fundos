using CaseItau.Domain.Entities;
using CaseItau.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CaseItau.Infra.Data;

/// <summary>
/// Entity Framework Core database context for the CaseItau application.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>Gets or sets the funds dataset.</summary>
    public DbSet<Fundo> Fundos { get; set; }

    /// <summary>Gets or sets the fund types dataset.</summary>
    public DbSet<TipoFundo> TiposFundo { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fundo>()
                    .Property(f => f.Cnpj)
                    .HasConversion(
                        cnpj => cnpj.Value,
                        value => new Cnpj(value))
                    .HasMaxLength(14)
                    .IsRequired();

        modelBuilder.Entity<Fundo>()
                    .HasIndex(f => f.Codigo)
                    .IsUnique();

        modelBuilder.Entity<Fundo>()
                    .HasIndex(f => f.Cnpj)
                    .IsUnique();

        modelBuilder.Entity<TipoFundo>().HasData(
            new TipoFundo { Codigo = 1, Nome = "RENDA FIXA" },
            new TipoFundo { Codigo = 2, Nome = "ACOES" },
            new TipoFundo { Codigo = 3, Nome = "MULTI MERCADO" }
        );
    }
}
