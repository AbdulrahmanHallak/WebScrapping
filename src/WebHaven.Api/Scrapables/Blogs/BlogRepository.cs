using System.Collections.Immutable; // ImmutableArray
using System.Data; // IDbConnection
using Dapper; // QueryAsync

namespace WebHaven.Api.Scrapables.Blogs;

internal class BlogRepository(IDbConnection connection)
{
    public async Task<ImmutableArray<BlogXPath>> GetBlogsXPath()
    {
        var query = "SELECT * FROM BlogsXPath WHERE Uri != 'https://weblog.west-wind.com'";
        IEnumerable<BlogXPath> result = await connection.QueryAsync<BlogXPath>(query);

        return ImmutableArray.Create(result.ToArray());
    }
}