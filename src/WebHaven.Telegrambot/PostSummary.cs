namespace WebHaven.Telegrambot;

internal record PostSummary(string? BlogUri, string? Uri, string? Title, string? Date, string? Description)
{
    public override string ToString()
    {
        return
        $"""
        {BlogUri}

                    {Title}


        {Description}


        {Uri}
        {Date}
        """;
    }
};