using Microsoft.EntityFrameworkCore;
using VaccineApp.Data.Entities;

namespace VaccineApp.Data.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Freezer> Freezers { get; set; }

    public virtual DbSet<FreezerStock> FreezerStocks { get; set; }

    public virtual DbSet<FreezerTemperature> FreezerTemperatures { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Vaccine> Vaccines { get; set; }

    public virtual DbSet<VaccineFreezer> VaccineFreezers { get; set; }

    public virtual DbSet<VaccineOrder> VaccineOrders { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Freezer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Freezers_pk");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<FreezerStock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FreezerStock_pk");

            entity.ToTable("FreezerStock");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.VaccineFreezer).WithMany(p => p.FreezerStocks)
                .HasForeignKey(d => d.VaccineFreezerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FreezerStockVaccine_Freezers_fk");
        });

        modelBuilder.Entity<FreezerTemperature>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FreezerTemperatures_pk");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Temperature).HasPrecision(5, 2);

            entity.HasOne(d => d.Freezer).WithMany(p => p.FreezerTemperatures)
                .HasForeignKey(d => d.FreezerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FreezerTemperaturesFreezers_fk");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Users_pk");

            //entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Address).HasColumnType("character varying");
            entity.Property(e => e.Name).HasColumnType("character varying");
            entity.Property(e => e.PhoneNumber).HasColumnType("character varying");
            entity.Property(e => e.Surname).HasColumnType("character varying");
            entity.Property(e => e.Username).HasColumnType("character varying");
        });       
        
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Roles_pk");

            //entity.Property(e => e.Id).UseIdentityAlwaysColumn(); 
            entity.Property(e => e.Name).HasColumnType("character varying");
            entity.Property(e => e.NormalizedName).HasColumnType("character varying");
            entity.Property(e => e.Description).HasColumnType("character varying"); 
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserRoles_pk");

            //entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
               .HasForeignKey(d => d.UserId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("UserRoles_Users_fk");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
               .HasForeignKey(d => d.RoleId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("UserRoles_Roles_fk");
        });

        modelBuilder.Entity<Vaccine>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Vaccines_pk"); 
            entity.HasIndex(e => e.Id, "VaccinesId_idx");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.CompanyName).HasColumnType("character varying");
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<VaccineFreezer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("VaccineFreezers_pk");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.Freezer).WithMany(p => p.VaccineFreezers)
                .HasForeignKey(d => d.FreezerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VaccineFreezers_Freezers_fk");

            entity.HasOne(d => d.Vaccine).WithMany(p => p.VaccineFreezers)
                .HasForeignKey(d => d.VaccineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VaccineFreezers_Vaccines_fk");
        });

        modelBuilder.Entity<VaccineOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("VaccineOrders_pk");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
           
            entity.HasOne(d => d.FreezerStock).WithMany(p => p.VaccineOrders)
                .HasForeignKey(d => d.FreezerStockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VaccineOrders_FreezerStock_fk");

            entity.HasOne(d => d.User).WithMany(p => p.VaccineOrders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("VaccineOrders_Users_fk");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AuditLogs_pk");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.Property(e => e.Username).HasColumnType("character varying");
            entity.Property(e => e.Action).HasColumnType("character varying");
            entity.Property(e => e.TableName).HasColumnType("character varying");
            entity.Property(e => e.PrimaryKey).HasColumnType("character varying");
            entity.Property(e => e.Changes).HasColumnType("character varying");

        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RefreshToken_pk");
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Token).HasColumnType("character varying"); 
            entity.Property(e => e.CreatedByIp).HasColumnType("character varying");
            entity.Property(e => e.RevokedByIp).HasColumnType("character varying");
            entity.Property(e => e.ReplacedByToken).HasColumnType("character varying");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OutboxMessage_pk");
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Type).HasColumnType("character varying"); 
            entity.Property(e => e.Payload).HasColumnType("character varying");
            entity.Property(e => e.Error).HasColumnType("character varying"); 

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}