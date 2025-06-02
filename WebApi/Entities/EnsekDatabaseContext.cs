using Microsoft.EntityFrameworkCore;

namespace WebApi.Entities;

public partial class EnsekDatabaseContext : DbContext
{
    public EnsekDatabaseContext()
    {
    }

    public EnsekDatabaseContext(DbContextOptions<EnsekDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CustomerAccount> CustomerAccounts { get; set; }

    public virtual DbSet<MeterReading> MeterReadings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomerAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId);

            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.HasKey(e => e.AccountId);

            entity.Property(e => e.AccountId).ValueGeneratedNever();
            entity.Property(e => e.MeterReadValue)
                .IsRequired()
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.MeterReadingDateTime).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
