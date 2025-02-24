using Functions.Article;
using Newtonsoft.Json;

namespace Functions.API;

public class ApiResponse
{
    [JsonProperty("batchcomplete")]
    public bool BatchComplete { get; set; }

    [JsonProperty("query")]
    public Query Query { get; set; }
}

public class Query
{
    [JsonProperty("pages")]
    public List<Page> Pages { get; set; }

    [JsonProperty("tokens")]
    public Tokens Tokens { get; set; }
}

public class Page
{
    [JsonProperty("pageid")]
    public int PageId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("contentmodel")]
    public string ContentModel { get; set; }

    [JsonProperty("pagelanguage")]
    public string PageLanguage { get; set; }

    [JsonProperty("pagelanguagehtmlcode")]
    public string PageLanguageHtmlCode { get; set; }

    [JsonProperty("pagelanguagedir")]
    public string PageLanguageDir { get; set; }

    [JsonProperty("touched")]
    public string Touched { get; set; }

    [JsonProperty("lastrevid")]
    public int LastRevId { get; set; }

    [JsonProperty("length")]
    public int Length { get; set; }

    [JsonProperty("protection")]
    public List<Protection> Protection { get; set; }

    [JsonProperty("displaytitle")]
    public string DisplayTitle { get; set; }

    [JsonProperty("revisions")]
    public List<Revision> Revisions { get; set; }
}

public class Revision
{
    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("slots")]
    public Dictionary<string, Slot> Slots { get; set; }
}

public class Slot
{
    [JsonProperty("contentmodel")]
    public string ContentModel { get; set; }

    [JsonProperty("contentformat")]
    public string ContentFormat { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }
}

public class Tokens
{
    [JsonProperty("csrftoken")]
    public string CsrfToken { get; set; }
}