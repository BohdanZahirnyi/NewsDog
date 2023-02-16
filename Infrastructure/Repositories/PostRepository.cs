using Infrastructure.EF;
using Infrastructure.Interfaces;
using Infrastructure.Models;

namespace Infrastructure.Repositories
{
    public class PostRepository : IRepository<Post>
    {
        private readonly PostsDbContext _dbContext;

        public PostRepository(PostsDbContext context)
        {
            _dbContext = context;
        }

        public List<Post> GetRange(List<int> ids)
        {
            return _dbContext.Posts.Where(x => ids.Contains(x.Id)).ToList();
        }

        public Post? Get(int id)
        {
            return _dbContext.Posts.Where(x => x.Id == id).FirstOrDefault();
        }
        public void Add(IEnumerable<Post> posts)
        {
             _dbContext.Posts.AddRange(posts);
        }

        public void Update(IEnumerable<Post> posts)
        {
            _dbContext.Posts.UpdateRange(posts);
        }

        public void DeleteRange(IEnumerable<Post> posts)
        {
            _dbContext.Posts.RemoveRange(posts);
        }

        public void SaveChanges(CancellationToken ct = default)
        {
             _dbContext.SaveChanges();
        }
    }
}
