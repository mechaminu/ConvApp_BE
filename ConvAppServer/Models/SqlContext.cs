using Microsoft.EntityFrameworkCore;

namespace ConvAppServer.Models
{
    public class SqlContext : DbContext
    {
        public SqlContext(DbContextOptions<SqlContext> options) : base(options) { }

        public DbSet<Posting> Postings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Posting>()
                .Property(b => b.Created)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Posting>()
                .HasOne(p => p.Creator)
                .WithMany(u => u.CreatedPostings)
                .HasForeignKey(p => p.CreatorId)
                .HasPrincipalKey(u => u.Id);

            modelBuilder.Entity<Posting>()
                .HasMany<PostingNode>(p => p.PostingNodes)
                .WithOne();

            modelBuilder.Entity<UserAuth>()
                .HasOne(ua => ua.User)
                .WithOne()
                .HasForeignKey<UserAuth>(ua => ua.UserId)
                .HasPrincipalKey<User>(u => u.Id);

            modelBuilder.Entity<UserAuth>()
                .HasKey(ua => ua.UserId);
        }
    }
}
