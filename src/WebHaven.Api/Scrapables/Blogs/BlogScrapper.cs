using System.Collections.Immutable; // ImmutableArray
using HtmlAgilityPack; // HtmlDocument

namespace WebHaven.Api.Scrapables.Blogs;

internal class BlogScrapper
{
    public async Task<ImmutableArray<PostSummary>> Scrape(BlogXPath blogXPath)
    {
        var client = new HtmlWeb();
        HtmlDocument doc = await client.LoadFromWebAsync(blogXPath.Uri.ToString());

        IEnumerable<HtmlNode> posts = doc.DocumentNode.SelectNodes(blogXPath.Posts);

        List<PostSummary> postSummaries = [];
        foreach (HtmlNode post in posts)
        {
            var uri = blogXPath.Uri + post.SelectSingleNode(blogXPath.PostsUri)?.Attributes?["href"]?.Value ?? string.Empty;
            string title = post.SelectSingleNode(blogXPath.PostsTitle)?.InnerHtml ?? string.Empty;
            string date = post.SelectSingleNode(blogXPath.PostsDate)?.InnerHtml ?? string.Empty;
            string description = post.SelectSingleNode(blogXPath.PostsDescription)?.InnerHtml ?? string.Empty;

            if (string.IsNullOrWhiteSpace(uri) || string.IsNullOrWhiteSpace(title)
             || string.IsNullOrWhiteSpace(date)
             || string.IsNullOrWhiteSpace(description))
                continue;

            var postSummary = new PostSummary(blogXPath.Uri, uri, title, date, description);
            postSummaries.Add(postSummary);
        }

        return ImmutableArray.Create(postSummaries.ToArray());
    }
}