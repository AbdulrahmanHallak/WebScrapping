using System.Collections.Immutable; // ImmutableArray
using Microsoft.Extensions.Caching.Memory; // IMemroyCache

namespace WebHaven.Api.Scrapables.Blogs;

internal class ScrapeBackgroundTask(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredKeyedService<Logger<ScrapeBackgroundTask>>(typeof(Logger<ScrapeBackgroundTask>));
            logger.LogInformation("Scraping websites");

            var repo = scope.ServiceProvider.GetRequiredService<BlogRepository>();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            var scrapper = scope.ServiceProvider.GetRequiredService<BlogScrapper>();

            ImmutableArray<BlogXPath> paths = await repo.GetBlogsXPath();

            var tasks = from path in paths
                        select scrapper.Scrape(path);
            var result = await Task.WhenAll(tasks);

            cache.CreateEntry(BlogConstants.BlogsCacheKey).AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            cache.Set(BlogConstants.BlogsCacheKey, result);

            logger.LogInformation("Added blogs to cache");
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}