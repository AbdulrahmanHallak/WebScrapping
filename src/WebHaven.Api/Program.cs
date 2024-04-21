using System.Data; // IDbConnection
using WebHaven.Api.Scrapables.Blogs; // BlogScrapper
using Microsoft.Data.SqlClient; // SqlConnection

namespace WebHaveni;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        string connection = builder.Configuration.GetConnectionString("SqlServerConnection") ??
                            throw new InvalidOperationException("Connection string cannot be null");

        builder.Services.AddSingleton<BlogScrapper>();
        builder.Services.AddSingleton<BlogRepository>();
        builder.Services.AddTransient<IDbConnection, SqlConnection>(_ => new SqlConnection(connection));
        builder.Services.AddMemoryCache();
        builder.Services.AddHostedService<ScrapeBackgroundTask>();

        var app = builder.Build();

        app.MapBlogEndpoints();

        app.Run();
    }
}
