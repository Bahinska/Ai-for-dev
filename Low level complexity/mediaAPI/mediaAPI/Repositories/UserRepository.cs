using Dapper;
using mediaAPI.Models;
using System.Data;

namespace mediaAPI.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int id);
        Task<int> SubscribeAsync(int followerId, int followeeId);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;

        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var sql = "SELECT * FROM users WHERE id = @Id;";
            return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<int> SubscribeAsync(int followerId, int followeeId)
        {
            const string followSql = "INSERT INTO followers (follower_id, followee_id) VALUES (@FollowerId, @FolloweeId);";
            return await _connection.ExecuteAsync(followSql, new { FollowerId = followerId, FolloweeId = followeeId });
        }
    }
}
