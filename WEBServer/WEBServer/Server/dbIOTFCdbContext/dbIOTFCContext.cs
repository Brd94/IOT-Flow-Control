// using System;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata;

// #nullable disable

// namespace WEBServer.Server.dbIOTFCdbContext
// {
//     public partial class dbIOTFCContext : DbContext
//     {
//         public dbIOTFCContext()
//         {
//         }

//         public dbIOTFCContext(DbContextOptions<dbIOTFCContext> options)
//             : base(options)
//         {
//         }

//         public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
//         public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
//         public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
//         public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
//         public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
//         public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
//         public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
//         public virtual DbSet<DeviceInfo> DeviceInfos { get; set; }
//         public virtual DbSet<EfmigrationsHistory> EfmigrationsHistories { get; set; }
//         public virtual DbSet<LocationInfo> LocationInfos { get; set; }
//         public virtual DbSet<Movement> Movements { get; set; }
//         public virtual DbSet<Probe> Probes { get; set; }
//         public virtual DbSet<UserLocation> UserLocations { get; set; }

//         protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         {
//             if (!optionsBuilder.IsConfigured)
//             {
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//                 optionsBuilder.UseMySQL("server=192.168.178.20;database=dbIOTFC;user id=admin;password=errata");
//             }
//         }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);


//             modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.Id).HasMaxLength(127));
//             modelBuilder.Entity<IdentityRole>(entity => entity.Property(m => m.ConcurrencyStamp).HasColumnType("varchar(256)"));

//             modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
//             {
//                 entity.Property(m => m.LoginProvider).HasMaxLength(127);
//                 entity.Property(m => m.ProviderKey).HasMaxLength(127);
//             });

//             modelBuilder.Entity<IdentityUserRole<string>>(entity =>
//             {
//                 entity.Property(m => m.UserId).HasMaxLength(127);
//                 entity.Property(m => m.RoleId).HasMaxLength(127);
//             });

//             modelBuilder.Entity<IdentityUserToken<string>>(entity =>
//             {
//                 entity.Property(m => m.UserId).HasMaxLength(127);
//                 entity.Property(m => m.LoginProvider).HasMaxLength(127);
//                 entity.Property(m => m.Name).HasMaxLength(127);
//             });

//             modelBuilder.Entity<AspNetRole>(entity =>
//             {
//                 entity.ToTable("AspNetRoles", "dbIOTFC");

//                 entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
//                     .IsUnique();

//                 entity.Property(e => e.Id).HasMaxLength(256);

//                 entity.Property(e => e.Name).HasMaxLength(256);

//                 entity.Property(e => e.NormalizedName).HasMaxLength(256);
//             });

//             modelBuilder.Entity<AspNetRoleClaim>(entity =>
//             {
//                 entity.ToTable("AspNetRoleClaims", "dbIOTFC");

//                 entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

//                 entity.Property(e => e.RoleId)
//                     .IsRequired()
//                     .HasMaxLength(256);

//                 entity.HasOne(d => d.Role)
//                     .WithMany(p => p.AspNetRoleClaims)
//                     .HasForeignKey(d => d.RoleId);
//             });

//             modelBuilder.Entity<AspNetUser>(entity =>
//             {
//                 entity.ToTable("AspNetUsers", "dbIOTFC");

//                 entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

//                 entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
//                     .IsUnique();

//                 entity.Property(e => e.Id).HasMaxLength(256);

//                 entity.Property(e => e.Email).HasMaxLength(256);

//                 entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

//                 entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

//                 entity.Property(e => e.UserName).HasMaxLength(256);
//             });

//             modelBuilder.Entity<AspNetUserClaim>(entity =>
//             {
//                 entity.ToTable("AspNetUserClaims", "dbIOTFC");

//                 entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

//                 entity.Property(e => e.UserId)
//                     .IsRequired()
//                     .HasMaxLength(256);

//                 entity.HasOne(d => d.User)
//                     .WithMany(p => p.AspNetUserClaims)
//                     .HasForeignKey(d => d.UserId);
//             });

//             modelBuilder.Entity<AspNetUserLogin>(entity =>
//             {
//                 entity.HasKey(e => new { e.LoginProvider, e.ProviderKey })
//                     .HasName("PRIMARY");

//                 entity.ToTable("AspNetUserLogins", "dbIOTFC");

//                 entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

//                 entity.Property(e => e.LoginProvider).HasMaxLength(256);

//                 entity.Property(e => e.ProviderKey).HasMaxLength(256);

//                 entity.Property(e => e.UserId)
//                     .IsRequired()
//                     .HasMaxLength(256);

//                 entity.HasOne(d => d.User)
//                     .WithMany(p => p.AspNetUserLogins)
//                     .HasForeignKey(d => d.UserId);
//             });

//             modelBuilder.Entity<AspNetUserRole>(entity =>
//             {
//                 entity.HasKey(e => new { e.UserId, e.RoleId })
//                     .HasName("PRIMARY");

//                 entity.ToTable("AspNetUserRoles", "dbIOTFC");

//                 entity.HasIndex(e => e.RoleId, "IX_AspNetUserRoles_RoleId");

//                 entity.Property(e => e.UserId).HasMaxLength(256);

//                 entity.Property(e => e.RoleId).HasMaxLength(256);

//                 entity.HasOne(d => d.Role)
//                     .WithMany(p => p.AspNetUserRoles)
//                     .HasForeignKey(d => d.RoleId);

//                 entity.HasOne(d => d.User)
//                     .WithMany(p => p.AspNetUserRoles)
//                     .HasForeignKey(d => d.UserId);
//             });

//             modelBuilder.Entity<AspNetUserToken>(entity =>
//             {
//                 entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name })
//                     .HasName("PRIMARY");

//                 entity.ToTable("AspNetUserTokens", "dbIOTFC");

//                 entity.Property(e => e.UserId).HasMaxLength(256);

//                 entity.Property(e => e.LoginProvider).HasMaxLength(256);

//                 entity.Property(e => e.Name).HasMaxLength(256);

//                 entity.HasOne(d => d.User)
//                     .WithMany(p => p.AspNetUserTokens)
//                     .HasForeignKey(d => d.UserId);
//             });

//             modelBuilder.Entity<DeviceInfo>(entity =>
//             {
//                 entity.HasKey(e => e.IdDevice)
//                     .HasName("PRIMARY");

//                 entity.ToTable("DeviceInfo", "dbIOTFC");

//                 entity.Property(e => e.IdDevice).HasColumnName("ID_Device");

//                 entity.Property(e => e.MacAddress)
//                     .IsRequired()
//                     .HasMaxLength(20)
//                     .HasColumnName("Mac_Address");
//             });

//             modelBuilder.Entity<EfmigrationsHistory>(entity =>
//             {
//                 entity.HasKey(e => e.MigrationId)
//                     .HasName("PRIMARY");

//                 entity.ToTable("__EFMigrationsHistory", "dbIOTFC");

//                 entity.Property(e => e.MigrationId).HasMaxLength(150);

//                 entity.Property(e => e.ProductVersion)
//                     .IsRequired()
//                     .HasMaxLength(32);
//             });

//             modelBuilder.Entity<LocationInfo>(entity =>
//             {
//                 entity.HasKey(e => e.IdLocation)
//                     .HasName("PRIMARY");

//                 entity.ToTable("LocationInfo", "dbIOTFC");

//                 entity.HasIndex(e => new { e.IdLocation, e.Status }, "IDX1");

//                 entity.Property(e => e.IdLocation).HasColumnName("ID_Location");

//                 entity.Property(e => e.Address)
//                     .IsRequired()
//                     .HasMaxLength(60);

//                 entity.Property(e => e.BusinessName)
//                     .IsRequired()
//                     .HasMaxLength(40)
//                     .HasColumnName("Business_Name");

//                 entity.Property(e => e.City)
//                     .IsRequired()
//                     .HasMaxLength(20);

//                 entity.Property(e => e.PeopleCount).HasColumnName("People_Count");

//                 entity.Property(e => e.PostalCode)
//                     .IsRequired()
//                     .HasMaxLength(10);

//                 entity.Property(e => e.Status).HasDefaultValueSql("'1'");
//             });

//             modelBuilder.Entity<Movement>(entity =>
//             {
//                 entity.HasKey(e => e.IdMove)
//                     .HasName("PRIMARY");

//                 entity.ToTable("Movements", "dbIOTFC");

//                 entity.HasIndex(e => e.IdDeviceFk, "Device_Constraints");

//                 entity.HasIndex(e => e.IdLocationFk, "Location_Constraints");

//                 entity.Property(e => e.IdMove).HasColumnName("ID_Move");

//                 entity.Property(e => e.IdDeviceFk).HasColumnName("ID_Device_FK");

//                 entity.Property(e => e.IdLocationFk).HasColumnName("ID_Location_FK");

//                 entity.Property(e => e.Type).HasColumnType("tinyint unsigned");

//                 entity.HasOne(d => d.IdDeviceFkNavigation)
//                     .WithMany(p => p.Movements)
//                     .HasForeignKey(d => d.IdDeviceFk)
//                     .HasConstraintName("Device_Constraints");

//                 entity.HasOne(d => d.IdLocationFkNavigation)
//                     .WithMany(p => p.Movements)
//                     .HasForeignKey(d => d.IdLocationFk)
//                     .HasConstraintName("Location_Constraints");
//             });

//             modelBuilder.Entity<Probe>(entity =>
//             {
//                 entity.HasKey(e => e.IdProbes)
//                     .HasName("PRIMARY");

//                 entity.ToTable("Probes", "dbIOTFC");

//                 entity.HasIndex(e => e.IdDeviceFk, "Probe_Constraints");

//                 entity.Property(e => e.IdProbes).HasColumnName("ID_Probes");

//                 entity.Property(e => e.IdDeviceFk).HasColumnName("ID_Device_FK");

//                 entity.HasOne(d => d.IdDeviceFkNavigation)
//                     .WithMany(p => p.Probes)
//                     .HasForeignKey(d => d.IdDeviceFk)
//                     .HasConstraintName("Probe_Constraints");
//             });

//             modelBuilder.Entity<UserLocation>(entity =>
//             {
//                 entity.HasKey(e => e.IdUl)
//                     .HasName("PRIMARY");

//                 entity.ToTable("UserLocations", "dbIOTFC");

//                 entity.HasIndex(e => e.IdLocation, "Location_Constraints_UL");

//                 entity.HasIndex(e => e.IdUser, "User_Constraints");

//                 entity.Property(e => e.IdUl).HasColumnName("ID_UL");

//                 entity.Property(e => e.IdLocation).HasColumnName("ID_Location");

//                 entity.Property(e => e.IdUser)
//                     .IsRequired()
//                     .HasMaxLength(256)
//                     .HasColumnName("ID_User");

//                 entity.HasOne(d => d.IdLocationNavigation)
//                     .WithMany(p => p.UserLocations)
//                     .HasForeignKey(d => d.IdLocation)
//                     .HasConstraintName("Location_Constraints_UL");

//                 entity.HasOne(d => d.IdUserNavigation)
//                     .WithMany(p => p.UserLocations)
//                     .HasForeignKey(d => d.IdUser)
//                     .HasConstraintName("User_Constraints");
//             });

//             OnModelCreatingPartial(modelBuilder);
//         }

//         partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
//     }
// }
