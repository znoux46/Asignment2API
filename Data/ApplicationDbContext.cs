using Microsoft.EntityFrameworkCore;

namespace Products_Management.API
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EntityConfiguration());
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Username).IsUnique();
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Username).HasMaxLength(100).IsRequired();
                b.Property(u => u.Email).HasMaxLength(255).IsRequired();
                b.Property(u => u.PasswordHash).IsRequired();
                b.Property(u => u.Role).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<CartItem>(b =>
            {
                b.ToTable("CartItems");
                b.HasKey(ci => ci.Id);
                b.Property(ci => ci.Quantity).HasDefaultValue(1);
            });

            modelBuilder.Entity<Order>(b =>
            {
                b.ToTable("Orders");
                b.HasKey(o => o.Id);
                b.Property(o => o.Status).HasMaxLength(20).HasDefaultValue("pending");
                b.Property(o => o.PaymentStatus).HasMaxLength(20).HasDefaultValue("pending");
                b.HasMany(o => o.Items)
                 .WithOne()
                 .HasForeignKey(oi => oi.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(b =>
            {
                b.ToTable("OrderItems");
                b.HasKey(oi => oi.Id);
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}