using System.Collections.ObjectModel;

namespace InsightsInn.Api.Scrapables.Blogs;

internal static class Endpoints
{
    public static WebApplication MapBlogEndpoints(this WebApplication app)
    {
        app.MapGet("api/blogs", async (BlogScrapper scrapper, BlogRepository repo) =>
        {
            ReadOnlyCollection<BlogXPath> paths = await repo.GetBlogsXPath();


            Dictionary<string, ReadOnlyCollection<PostSummary>> result = [];
            foreach (var item in paths)
            {
                ReadOnlyCollection<PostSummary> posts = await scrapper.Scrape(item);
                result.Add(item.Uri, posts);
            }

            return TypedResults.Ok<IDictionary<string, ReadOnlyCollection<PostSummary>>>(result);
        });

        return app;
    }
}