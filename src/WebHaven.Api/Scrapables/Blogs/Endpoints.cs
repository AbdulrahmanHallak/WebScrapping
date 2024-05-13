using System.Collections.Immutable; // ImmutableArray
using Microsoft.AspNetCore.Mvc; // FromQuery
using Microsoft.Extensions.Caching.Memory; // IMemoryCache

namespace WebHaven.Api.Scrapables.Blogs;

internal static class Endpoints
{
    public static WebApplication MapBlogEndpoints(this WebApplication app)
    {
        app.MapGet("api/blogs", async ([FromQuery] string? id, BlogScrapper scrapper, BlogRepository repo, IMemoryCache cache) =>
        {
            if (id is null)
                return Results.BadRequest();
            if (cache.TryGetValue<ImmutableArray<PostSummary>>(id, out var cachedResult))
                return TypedResults.Ok(cachedResult);

            var path = await repo.GetBlogXPath(id);
            if (path is null)
                return Results.BadRequest();
            var result = await scrapper.Scrape(path);

            return TypedResults.Ok(cachedResult);
        });

        // app.MapGet("api/blogs", async ([FromQuery] string id, BlogScrapper scrapper, BlogRepository repo, IMemoryCache cache) =>
        // {
        //     if (cache.TryGetValue<IDictionary<string, ImmutableArray<PostSummary>>>(id, out var cachedResult))
        //         return TypedResults.Ok(cachedResult);

        //     var path = await repo.GetBlogXPath(id);
        //     if (path is null)
        //         return Results.BadRequest();

        //     var posts = await scrapper.Scrape(path);

        //     return TypedResults.Ok(posts);
        // });

        return app;
    }
}