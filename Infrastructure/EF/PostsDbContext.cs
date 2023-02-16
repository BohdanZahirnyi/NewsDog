using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EF
{
    public class PostsDbContext : DbContext
    {

        public DbSet<Post> Posts { get; set; }
        public PostsDbContext(DbContextOptions<PostsDbContext> options) : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }
}
