using System.Collections.Immutable; // ImmutableArray
using Dapper; // QueryAsync
using Microsoft.Data.SqlClient; // SqlConnection

namespace WebHaven.Api.Scrapables.Blogs;

internal class BlogRepository(ConnectionString connString)
{
    public async Task<ImmutableArray<BlogXPath>> GetBlogsXPath()
    {
        var query = "SELECT * FROM BlogsXPath WHERE Uri != 'https://weblog.west-wind.com'";
        using var connection = new SqlConnection(connString);
        IEnumerable<BlogXPath> result = await connection.QueryAsync<BlogXPath>(query);

        return ImmutableArray.Create(result.ToArray());
    }
}