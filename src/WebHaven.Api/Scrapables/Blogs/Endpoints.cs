using System.Collections.Immutable; // ImmutableArray
using Microsoft.Extensions.Caching.Memory; // IMemoryCache

namespace WebHaven.Api.Scrapables.Blogs;

internal static class Endpoints
{
    public static WebApplication MapBlogEndpoints(this WebApplication app)
    {
        app.MapGet("api/blogs", async (BlogScrapper scrapper, BlogRepository repo, IMemoryCache cache) =>
        {
            // TODO: Make retrieving from cache the repo responsibility
            if (cache.TryGetValue<IDictionary<string, ImmutableArray<PostSummary>>>(BlogConstants.BlogsCacheKey, out var cachedResult))
                return TypedResults.Ok(cachedResult);

            ImmutableArray<BlogXPath> paths = await repo.GetBlogsXPath();

            Dictionary<string, ImmutableArray<PostSummary>> result = [];
            foreach (var item in paths)
            {
                ImmutableArray<PostSummary> posts = await scrapper.Scrape(item);
                result.Add(item.Uri, posts);
            }

            return TypedResults.Ok<IDictionary<string, ImmutableArray<PostSummary>>>(result);
        });

        return app;
    }
}