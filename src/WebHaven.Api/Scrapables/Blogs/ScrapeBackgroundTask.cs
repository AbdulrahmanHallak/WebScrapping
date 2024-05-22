using System.Collections.Immutable; // ImmutableArray
using Microsoft.Extensions.Caching.Memory; // IMemoryCache

namespace WebHaven.Api.Scrapables.Blogs;

internal class ScrapeBackgroundTask(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (IServiceScope scope = scopeFactory.CreateScope());
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<ScrapeBackgroundTask>();
                logger.LogInformation("Scraping websites");

                var repo = scope.ServiceProvider.GetRequiredService<BlogRepository>();
                var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                var scrapper = scope.ServiceProvider.GetRequiredService<BlogScrapper>();

                ImmutableArray<BlogXPath> paths = await repo.GetBlogsXPath();
                var scrapeTasks = paths.Select(path => new { path.Uri, Task = scrapper.Scrape(path) }).ToArray();
                await Task.WhenAll(scrapeTasks.Select(x => x.Task));

                foreach (var item in scrapeTasks)
                    cache.Set(item.Uri, await item.Task);

                logger.LogInformation("Added blogs to cache");
            }
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
