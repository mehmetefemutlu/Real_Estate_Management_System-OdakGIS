using Microsoft.EntityFrameworkCore;
using srs.Entity;

namespace srs.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // database tables
    public DbSet<City> cities { get; set; }
    public DbSet<District> districts { get; set; }
    public DbSet<Neighbourhood> neighbourhoods { get; set; }
    public DbSet<Property> properties { get; set; }
    public DbSet<Polygon> polygons { get; set; }
    public DbSet<User> users { get; set; }
    public DbSet<Log> logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //relational model for city
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.cityName).IsRequired().HasMaxLength(100);

            entity.HasMany(e => e.districts)
                .WithOne(d => d.city)
                .HasForeignKey(d => d.cityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        //relational model for district
        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.districtName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.postalCode).IsRequired().HasMaxLength(20);

            entity.HasMany(e => e.neighbourhoods)
                .WithOne(n => n.district)
                .HasForeignKey(n => n.districtId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // relational model for neighbourhood
        modelBuilder.Entity<Neighbourhood>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.name).IsRequired().HasMaxLength(100);

            entity.HasMany(e => e.properties)
                .WithOne(p => p.neighbourhood)
                .HasForeignKey(p => p.neighbourhoodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // relational model for property
        modelBuilder.Entity<Property>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.description).HasMaxLength(2000);
            entity.Property(e => e.address).IsRequired().HasMaxLength(300);
            entity.Property(e => e.price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.area).HasColumnType("double precision");
            entity.Property(e => e.parcelNumber).HasMaxLength(50);
            entity.Property(e => e.lotNumber).HasMaxLength(50);
            entity.Property(e => e.createdAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.owner)
                .WithMany(u => u.properties)
                .HasForeignKey(e => e.ownerId)
                .OnDelete(DeleteBehavior.Restrict);

            //create B+ Tree Indices for necessary fields
            entity.HasIndex(e => e.type);
            entity.HasIndex(e => e.price);
            entity.HasIndex(e => e.createdAt);
            entity.HasIndex(e => e.ownerId);
            entity.HasIndex(e => e.parcelNumber);
            entity.HasIndex(e => e.lotNumber);
        });

        // relational model for user
        modelBuilder.Entity<Polygon>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.ownerId).IsRequired();
            entity.Property(e => e.label).IsRequired().HasMaxLength(100);
            entity.Property(e => e.coordinates).IsRequired();
            entity.Property(e => e.createdAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.updatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.owner)
                .WithMany(u => u.polygons)
                .HasForeignKey(e => e.ownerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ownerId);
            entity.HasIndex(e => e.label);
        });

        // relational model for user
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.firstname).IsRequired().HasMaxLength(50);
            entity.Property(e => e.lastname).IsRequired().HasMaxLength(50);
            entity.Property(e => e.email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.passwordHash).IsRequired();
            entity.Property(e => e.passwordSalt).IsRequired();
            entity.Property(e => e.createdAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.email).IsUnique();

            entity.HasMany(e => e.logs)
                .WithOne(l => l.user)
                .HasForeignKey(l => l.userId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // relational model for log
        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.id);
            entity.Property(e => e.message).IsRequired().HasMaxLength(500);
            entity.Property(e => e.description).HasMaxLength(1000);
            entity.Property(e => e.createdAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.createdAt);
            entity.HasIndex(e => e.operationType);
            entity.HasIndex(e => e.userId);
        });
    }
}
