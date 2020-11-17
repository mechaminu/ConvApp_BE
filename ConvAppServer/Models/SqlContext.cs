using System;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ConvAppServer.Models
{
    public class SqlContext : DbContext
    { 
        public SqlContext(DbContextOptions<SqlContext> options) : base(options) {}

        public DbSet<Posting> Postings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        // 추후 feedback db로 분리할 것들...
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 포스팅 및 포스팅 노드 테이블 정의
            modelBuilder.Entity<Posting>(b =>
            {
                b
                .HasMany(p => p.PostingNodes)
                .WithOne();
            });

            modelBuilder.Entity<PostingNode>()
                .HasKey(pn => new { pn.PostingId, pn.OrderIndex });

            // 유저 및 유저인증정보 테이블 정의
            modelBuilder.Entity<User>(b =>
            {
                b
                .HasMany(u => u.CreatedPostings)
                .WithOne()
                .HasForeignKey(p => p.CreatorId)
                .HasPrincipalKey(u => u.Id);
            });
                

            modelBuilder.Entity<UserAuth>(b =>
            {
                b
                .HasOne(ua => ua.User)
                .WithOne()
                .HasForeignKey<UserAuth>(ua => ua.UserId)
                .HasPrincipalKey<User>(u => u.Id);

                b.HasKey(ua => ua.UserId);
            });

            // 피드백 및 코멘트 테이블 정의
            modelBuilder.Entity<Like>(b =>
            {
                b
                .HasOne<User>()
                .WithMany(u => u.CreatedLikes)
                .HasForeignKey(l => l.CreatorId)
                .HasPrincipalKey(u => u.Id);

                b.HasKey(l => new { l.ParentType, l.ParentId });
            });

            modelBuilder.Entity<Comment>(b =>
            {
                b
                .HasKey(c => new { c.ParentId, c.ParentType, c.CreatedDate });

                b
                .HasOne<User>()
                .WithMany(u => u.CreatedComments)
                .HasForeignKey(c => c.CreatorId)
                .HasPrincipalKey(u => u.Id);
            });
                
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var elem = entityEntry.Entity;
                ((BaseEntity)elem).ModifiedDate = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseEntity)elem).CreatedDate = DateTime.UtcNow;

                    if (elem is Feedbackable)
                    {
                        var elemf = (Feedbackable)elem;

                        elemf.ViewCount = 0;
                        elemf.CommentCount = 0;
                        elemf.LikeCount = 0;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
