using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WEBServer.Server.Models.Entities;

#nullable disable

namespace WEBServer.Server.Services.Infrastructure
{
    public partial class dbIOTFCdbContext : IdentityDbContext
    {
        public dbIOTFCdbContext()
        {
        }

        public dbIOTFCdbContext(DbContextOptions<dbIOTFCdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DeviceInfo> DeviceInfos { get; set; }
        public virtual DbSet<LocationInfo> LocationInfos { get; set; }
        public virtual DbSet<Movement> Movements { get; set; }
        public virtual DbSet<Probe> Probes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.

                string connectionString = "server=192.168.178.20;database=dbIOTFC;user id=root;password=Ogpdmllf2!"; //Da portare su file di config.
                optionsBuilder.UseMySQL(connectionString);

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.Id).HasMaxLength(127));
            modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.ConcurrencyStamp).HasColumnType("varchar(256)"));

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(m => m.LoginProvider).HasMaxLength(127);
                entity.Property(m => m.ProviderKey).HasMaxLength(127);
            });

            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.Property(m => m.UserId).HasMaxLength(127);
                entity.Property(m => m.RoleId).HasMaxLength(127);
            });

            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(m => m.UserId).HasMaxLength(127);
                entity.Property(m => m.LoginProvider).HasMaxLength(127);
                entity.Property(m => m.Name).HasMaxLength(127);
            });

            
            
            
            modelBuilder.Entity<DeviceInfo>(entity =>
            {
                entity.HasKey(e => e.IdDevice)
                    .HasName("PRIMARY");

                entity.ToTable("DeviceInfo", "dbIOTFC");

                entity.Property(e => e.IdDevice).HasColumnName("ID_Device");

                entity.Property(e => e.MacAddress)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("Mac_Address");
            });

            modelBuilder.Entity<LocationInfo>(entity =>
            {
                entity.HasKey(e => e.IdLocation)
                    .HasName("PRIMARY");

                entity.ToTable("LocationInfo", "dbIOTFC");

                entity.HasIndex(e => new { e.IdLocation, e.Status }, "IDX1");

                entity.Property(e => e.IdLocation).HasColumnName("ID_Location");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(e => e.BusinessName)
                    .IsRequired()
                    .HasMaxLength(40)
                    .HasColumnName("Business_Name");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.PeopleCount).HasColumnName("People_Count");

                entity.Property(e => e.PostalCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Status)
                    .HasDefaultValueSql("'1'")
                    .HasComment("0 - Disattivo,1 - Attivo");
            });

            modelBuilder.Entity<Movement>(entity =>
            {
                entity.HasKey(e => e.IdMove)
                    .HasName("PRIMARY");

                entity.ToTable("Movements", "dbIOTFC");

                entity.HasIndex(e => e.IdDeviceFk, "Device_Constraints");

                entity.HasIndex(e => e.IdLocationFk, "Location_Constraints");

                entity.Property(e => e.IdMove).HasColumnName("ID_Move");

                entity.Property(e => e.IdDeviceFk).HasColumnName("ID_Device_FK");

                entity.Property(e => e.IdLocationFk).HasColumnName("ID_Location_FK");

                entity.Property(e => e.Type)
                    .HasColumnType("tinyint unsigned")
                    .HasComment("0 - ND,1 - FI,2 - M,3 - DM");

                entity.HasOne(d => d.IdDeviceFkNavigation)
                    .WithMany(p => p.Movements)
                    .HasForeignKey(d => d.IdDeviceFk)
                    .HasConstraintName("Device_Constraints");

                entity.HasOne(d => d.IdLocationFkNavigation)
                    .WithMany(p => p.Movements)
                    .HasForeignKey(d => d.IdLocationFk)
                    .HasConstraintName("Location_Constraints");
            });

            modelBuilder.Entity<Probe>(entity =>
            {
                entity.HasKey(e => e.IdProbes)
                    .HasName("PRIMARY");

                entity.ToTable("Probes", "dbIOTFC");

                entity.HasIndex(e => e.IdDeviceFk, "Probe_Constraints");

                entity.Property(e => e.IdProbes).HasColumnName("ID_Probes");

                entity.Property(e => e.IdDeviceFk).HasColumnName("ID_Device_FK");

                entity.HasOne(d => d.IdDeviceFkNavigation)
                    .WithMany(p => p.Probes)
                    .HasForeignKey(d => d.IdDeviceFk)
                    .HasConstraintName("Probe_Constraints");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
