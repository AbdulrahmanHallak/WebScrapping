using System.Collections.ObjectModel; // ReadOnlyCollection
using Microsoft.Extensions.Caching.Memory; // IMemroyCache

namespace InsightsInn.Api.Scrapables.Blogs;

internal class ScrapeBackgroundTask(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Inside background task");

            using IServiceScope scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<BlogRepository>();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            var scrapper = scope.ServiceProvider.GetRequiredService<BlogScrapper>();

            ReadOnlyCollection<BlogXPath> paths = await repo.GetBlogsXPath();

            Dictionary<string, ReadOnlyCollection<PostSummary>> result = [];
            foreach (var item in paths)
            {
                ReadOnlyCollection<PostSummary> posts = await scrapper.Scrape(item);
                result.Add(item.Uri, posts);
            }

            cache.CreateEntry(BlogConstants.BlogsCacheKey).AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            cache.Set(BlogConstants.BlogsCacheKey, result);

            Console.WriteLine("Added blogs to cache");

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}