    // Add required namespaces
    using System.Data;
    using System.Threading.Tasks;
    using Dapper;
    using mediaAPI.Models;
namespace mediaAPI.Repositories
{
    public interface IPostRepository
    {
        Task<Post> GetPostByIdAsync(int id);
        Task<int> CreatePostAsync(Post post);
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<int> LikePostAsync(int id);
    }
    public class PostRepository : IPostRepository
    {
        private readonly IDbConnection _connection;

        public PostRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            var sql = "SELECT * FROM posts WHERE id = @Id;";
            return await _connection.QueryFirstOrDefaultAsync<Post>(sql, new { Id = id });
        }

        public async Task<int> CreatePostAsync(Post post)
        {
            var sql = "INSERT INTO posts (title, body, author) VALUES (@Title, @Body, @Author) RETURNING id;";
            return await _connection.ExecuteAsync(sql, post);
        }

        public async Task<int> LikePostAsync(int id)
        {
            const string updateLikesSql = "UPDATE posts SET likes = likes + 1 WHERE id = @postId;";
            return await _connection.ExecuteAsync(updateLikesSql, new { postId = id });
        }

        public async Task<IEnumerable<Post>> GetAllPostsAsync()
        {
            const string getAllSql = "SELECT * FROM posts;";
            return await _connection.QueryAsync<Post>(getAllSql);
        }
    }
}
