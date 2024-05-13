using System.Collections.Immutable; // ImmutableArray
using Dapper; // QueryAsync
using Microsoft.Data.SqlClient; // SqlConnection

namespace WebHaven.Api.Scrapables.Blogs;

internal class BlogRepository(ConnectionString connString)
{
    public async Task<ImmutableArray<BlogXPath>> GetBlogsXPath()
    {
        var query = "SELECT * FROM BlogsXPath";
        using var connection = new SqlConnection(connString);
        IEnumerable<BlogXPath> result = await connection.QueryAsync<BlogXPath>(query);

        return ImmutableArray.Create(result.ToArray());
    }
    public async Task<BlogXPath?> GetBlogXPath(string id)
    {
        var query = "SELECT * FROM BlogsXPath WHERE Uri = @id";
        using var connection = new SqlConnection(connString);
        BlogXPath? result = await connection.QueryFirstOrDefaultAsync<BlogXPath>(query, id);

        return result;
    }

}