using WebHaven.Api.Scrapables.Blogs; // BlogScrapper

namespace WebHaven.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connection = new ConnectionString(builder.Configuration.GetConnectionString("SqlServerConnection") ??
                            throw new InvalidOperationException("Connection string cannot be null"));
        builder.Services.AddSingleton<ConnectionString>(_ => connection);
        builder.Services.AddSingleton<BlogScrapper>();
        builder.Services.AddSingleton<BlogRepository>();
        builder.Services.AddMemoryCache();
        builder.Services.AddHostedService<ScrapeBackgroundTask>();

        var app = builder.Build();

        app.MapBlogEndpoints();

        app.Run();
    }
}
