using Gruppe5Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace Gruppe5Projekt.Data;

/// <summary>
/// EF-Core-DbContext für die Verwaltung von MC-Klausuren.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Lehrveranstaltung> Lehrveranstaltungen => Set<Lehrveranstaltung>();
    public DbSet<Kapitel> Kapitel => Set<Kapitel>();
    public DbSet<MCFrage> MCFragen => Set<MCFrage>();
    public DbSet<MCAntwortOption> MCAntwortOptionen => Set<MCAntwortOption>();
    public DbSet<Pruefung> Pruefungen => Set<Pruefung>();
    public DbSet<PruefungMCFrage> PruefungMCFragen => Set<PruefungMCFrage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Niveau als Text in der DB speichern (besser lesbar als int)
        modelBuilder.Entity<Lehrveranstaltung>()
            .Property(l => l.Niveau)
            .HasConversion<string>();

        // Lehrveranstaltung 1:n Kapitel
        modelBuilder.Entity<Kapitel>()
            .HasOne(k => k.Lehrveranstaltung)
            .WithMany(l => l.Kapitel)
            .HasForeignKey(k => k.LehrveranstaltungId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lehrveranstaltung 1:n Pruefung
        modelBuilder.Entity<Pruefung>()
            .HasOne(p => p.Lehrveranstaltung)
            .WithMany(l => l.Pruefungen)
            .HasForeignKey(p => p.LehrveranstaltungId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kapitel 1:n MCFrage
        modelBuilder.Entity<MCFrage>()
            .HasOne(f => f.Kapitel)
            .WithMany(k => k.MCFragen)
            .HasForeignKey(f => f.KapitelId)
            .OnDelete(DeleteBehavior.Cascade);

        // MCFrage 1:n MCAntwortOption
        modelBuilder.Entity<MCAntwortOption>()
            .HasOne(a => a.MCFrage)
            .WithMany(f => f.AntwortOptionen)
            .HasForeignKey(a => a.MCFrageId)
            .OnDelete(DeleteBehavior.Cascade);

        // MCFrage n:m Pruefung über PruefungMCFrage
        // Zusammengesetzter Primärschlüssel aus beiden Fremdschlüsseln
        modelBuilder.Entity<PruefungMCFrage>()
            .HasKey(pf => new { pf.PruefungId, pf.MCFrageId });

        modelBuilder.Entity<PruefungMCFrage>()
            .HasOne(pf => pf.Pruefung)
            .WithMany(p => p.PruefungMCFragen)
            .HasForeignKey(pf => pf.PruefungId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PruefungMCFrage>()
            .HasOne(pf => pf.MCFrage)
            .WithMany(f => f.PruefungMCFragen)
            .HasForeignKey(pf => pf.MCFrageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
