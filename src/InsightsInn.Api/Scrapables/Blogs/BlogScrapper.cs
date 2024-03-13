using System.Collections.ObjectModel; // ReadOnlyCollection
using HtmlAgilityPack; // HtmlDocument

namespace InsightsInn.Api.Scrapables.Blogs;

internal class BlogScrapper
{
    public async Task<ReadOnlyCollection<PostSummary>> Scrape(BlogXPath blogXPath)
    {
        var client = new HtmlWeb();
        HtmlDocument doc = await client.LoadFromWebAsync(blogXPath.Uri.ToString());

        IEnumerable<HtmlNode> posts = doc.DocumentNode.SelectNodes(blogXPath.Posts).Skip(2);

        List<PostSummary> postSummaries = [];
        foreach (HtmlNode post in posts)
        {
            var uri = blogXPath.Uri + post.SelectSingleNode(blogXPath.PostsUri).Attributes["href"].Value;
            string title = post.SelectSingleNode(blogXPath.PostsTitle).InnerHtml;
            string date = post.SelectSingleNode(blogXPath.PostsDate).InnerHtml;
            string description = post.SelectSingleNode(blogXPath.PostsDescription).InnerHtml;

            var postSummary = new PostSummary(blogXPath.Uri, uri, title, date, description);
            postSummaries.Add(postSummary);
        }

        return new ReadOnlyCollection<PostSummary>(postSummaries);
    }
}