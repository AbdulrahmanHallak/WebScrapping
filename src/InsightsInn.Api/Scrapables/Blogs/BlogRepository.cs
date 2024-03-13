using System.Collections.ObjectModel; // ReadOnlyCollection
using System.Data; // IDbConnection
using Dapper; // QueryAsync

namespace InsightsInn.Api.Scrapables.Blogs;

internal class BlogRepository(IDbConnection connection)
{
    public async Task<ReadOnlyCollection<BlogXPath>> GetBlogsXPath()
    {
        var query = "SELECT * FROM BlogsXPath WHERE Uri = 'https://andrewlock.net'";
        IEnumerable<BlogXPath> result = await connection.QueryAsync<BlogXPath>(query);

        return new ReadOnlyCollection<BlogXPath>(result.ToList());
    }
}