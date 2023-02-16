using Infrastructure.Models;

namespace Infrastructure.Interfaces
{
    public interface IRepository<TEntity>
        where TEntity : class
    {
        public TEntity? Get(int id);
        public List<TEntity> GetRange(List<int> ids);
        public void Add(IEnumerable<TEntity> entities);
        public void Update(IEnumerable<Post> posts);
        public void DeleteRange(IEnumerable<TEntity> entities);
        public void SaveChanges(CancellationToken ct = default);
    }
}
