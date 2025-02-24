using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;
using Functions.Article;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace Functions.API;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Wiki _wiki;
    private CookieContainer CookieContainer { get; set; } = new CookieContainer();
    
    private string ApiUrl { get; set; }
    
    public ApiClient(Wiki wiki)
    {
        _wiki = wiki;
        
        var handler = new HttpClientHandler
        {
            CookieContainer = CookieContainer,
            UseCookies = true,
            AllowAutoRedirect = true
        };

        _httpClient = new HttpClient(handler);
        
        // set the user agent per WMF Guidelines
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_wiki.UserAgent);
    }
    
    /// <summary>
    /// Determine the script path of the wiki so that we know where to send API calls etc.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task DetermineScriptPathAndLoadSiteInfoAsync()
    {
        string[] apiPaths = { "/api.php", "/w/api.php" };

        foreach (string apiPath in apiPaths)
        {
            string testUrl = $"{_wiki.Url}{apiPath}";
            var fullUrl = $"{testUrl}?action=query&meta=siteinfo&siprop=general|namespaces&format=json";
            
            HttpResponseMessage response = await _httpClient.GetAsync(fullUrl);

            if (response.IsSuccessStatusCode)
            {
                ApiUrl = testUrl;
                await ParseSiteInfo(response);
                return;
            }
        }

        throw new InvalidOperationException("Failed to locate API endpoint; /api.php and /w/api.php were checked.");
    }

    /// <summary>
    /// Parse the site information so we can set stuff on the class; this feels wrong being in here?
    /// </summary>
    /// <param name="response"></param>
    private async Task ParseSiteInfo(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement query = doc.RootElement.GetProperty("query");
        JsonElement general = query.GetProperty("general");

        // Set script path from API response, either /w or "" (blank)
        _wiki.ScriptPath = general.GetProperty("scriptpath").GetString();
        
        // Set needed wiki properties
        _wiki.Sitename = general.GetProperty("sitename").GetString();
        _wiki.LangCode = general.GetProperty("lang").GetString();

        // Load the namespaces as well
        var namespaces = query.GetProperty("namespaces");
        foreach (JsonProperty ns in namespaces.EnumerateObject())
        {
            if (int.TryParse(ns.Name, out int id))
            {
                var nsData = ns.Value;
                _wiki.CanonicalNamespaces[id] = nsData.GetProperty("*").GetString() + ":";
            }
        }
    }

    /// <summary>
    /// Return the user information for the particular wiki we are operating on.
    /// This lists whether or not they are blocked, an admin, etc.
    /// </summary>
    /// <returns></returns>
    public async Task<string> LoginUserAsync(string username, string password)
    {
        string? loginToken = await GetTokenAsync("login");

        if (string.IsNullOrWhiteSpace(loginToken))
        {
            throw new InvalidOperationException("Failed to retrieve login token.");
        }

        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "action", "login" },
            { "format", "json" },
            { "lgname", username },
            { "lgpassword", password }, 
            { "lgtoken", loginToken } 
        };

        var encodedContent = new FormUrlEncodedContent(parameters);
    
        HttpResponseMessage response = await _httpClient.PostAsync(ApiUrl, encodedContent);
    
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to log in. Status code: {response.StatusCode}");
        }
    
        string jsonResponse = await response.Content.ReadAsStringAsync();
        
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("login", out JsonElement loginElement) &&
            loginElement.TryGetProperty("result", out JsonElement resultElement))
        {
            string result = resultElement.GetString() ?? "";

            if (result == "Success")
            {
                return "Login successful!";
            }
            else
            {
                return $"Login failed. Reason: {result}";
            }
        }

        throw new InvalidOperationException("Unexpected login response format.");
    }


    /// <summary>
    /// Return a token for usage for doing a subsequent action (like logging in etc).
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<string?> GetTokenAsync(string type)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "action", "query" },
            { "meta", "tokens" },
            { "format", "json" },
            { "type", type }
        };


        var queryString = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var requestUrl = $"{ApiUrl}?{queryString}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to retrieve token. Status code: {response.StatusCode}");
        }
        
        string jsonResponse = await response.Content.ReadAsStringAsync();

        // try and extract our token from the api response, hopefully MediaWiki returned it PROPERLY
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        JsonElement root = doc.RootElement;
        
        if (root.TryGetProperty("query", out JsonElement queryElement) &&
            queryElement.TryGetProperty("tokens", out JsonElement tokensElement))
        {
            // MediaWiki returns the response as: 
            // {
            //     "batchcomplete": "",
            //     "query": {
            //         "tokens": {
            //             "logintoken": "9ed1499d99c0c34c73faa07157b3b6075b427365+\\"
            //         }
            //     }
            // }
            // where the key is whatever the token we asked for  + "token". Lets try and get that
            // see more: https://www.mediawiki.org/wiki/API:Tokens#Example
            string tokenKey = $"{type}token"; 
            if (tokensElement.TryGetProperty(tokenKey, out JsonElement tokenElement))
            {
                return tokenElement.GetString();
            }
        }
        
        // something went wrong. 
        // NOTE: the caller is responsible for checking whether this method has returned a value or not
        // do not always assume that it will. If something goes wrong this function WILL RETURN NULL (or throw an exception)
        return null; 
    }

    /// <summary>
    /// Fetch the user information for the currently logged in user
    /// This does fall back to using an anon user if we failed to log in, maybe the login method needs to
    /// bail or something if that is the case?
    /// </summary>
    /// <exception cref="HttpRequestException"></exception>
    public async Task FetchUserInformationAsync()
    {
        var parameters = new Dictionary<string, string>
        {
            { "action", "query" },
            { "meta", "userinfo" },
            { "uiprop", "blockinfo|groups|rights" },
            { "format", "json" }
        };
        
        var queryString = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var requestUrl = $"{ApiUrl}?{queryString}";
        
        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to retrieve user information. Status code: {response.StatusCode}");
        }
        
        string jsonResponse = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonResponse);

        var userInfo = jsonDoc.RootElement.GetProperty("query").GetProperty("userinfo");

        string username = userInfo.GetProperty("name").GetString();
        bool isBlocked = userInfo.TryGetProperty("blockinfo", out _) ? true : false;
        List<string> groups = userInfo.GetProperty("groups").EnumerateArray().Select(g => g.GetString()).ToList();
        List<string> rights = userInfo.GetProperty("rights").EnumerateArray().Select(r => r.GetString()).ToList();
        bool isBot = groups.Contains("bot");

        _wiki.User = new User(username, isBot, isBlocked, groups, rights);
    }

    /// <summary>
    /// Get all of the pages within a category
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
   public async Task<List<string>> GetPagesInCategoryAsync(string category) 
   {
    // add the category prefix if it isn't there
    // see: https://www.mediawiki.org/wiki/API:Categorymembers for more info
    if (!category.StartsWith("Category:"))
    {
        category = "Category:" + category;
    }

    var pages = new List<string>();
    string? cmContinue = null;

    do
    {
        var parameters = new Dictionary<string, string>
        {
            { "action", "query" },
            { "list", "categorymembers" },
            { "cmtitle", category },
            { "cmtype", "page" },
            { "cmlimit", "500" }, 
            { "format", "json" }
        };

        if (!string.IsNullOrEmpty(cmContinue))
        {
            parameters["cmcontinue"] = cmContinue;
        }

        string queryString = string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        string requestUrl = $"{ApiUrl}?{queryString}";

        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to retrieve category members. Status code: {response.StatusCode}");
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        JsonElement root = doc.RootElement;

        if (root.TryGetProperty("query", out JsonElement queryElement) &&
            queryElement.TryGetProperty("categorymembers", out JsonElement membersArray))
        {
            foreach (JsonElement member in membersArray.EnumerateArray())
            {
                if (member.TryGetProperty("title", out JsonElement titleElement))
                {
                    pages.Add(titleElement.GetString() ?? string.Empty);
                }
            }
        }

        // Check if there's a continuation parameter if so we need to go again
        if (root.TryGetProperty("continue", out JsonElement continueElement) &&
            continueElement.TryGetProperty("cmcontinue", out JsonElement cmContinueElement))
        {
            cmContinue = cmContinueElement.GetString();
        }
        else
        {
            cmContinue = null;
        }

    } while (!string.IsNullOrEmpty(cmContinue));

    return pages;
    }

    /// <summary>
    /// Try and get the information about the page we want to work on, and create an instance of the Article class
    /// so that we may better understand its structure (and also get the content etc.)
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<Article.Article> GetArticleAsync(string title)
    {

        Dictionary<String, String> parameters = new Dictionary<string, string>
        {
            { "action", "query" },
            { "format", "json" },
            { "prop", "info|revisions" },
            { "meta", "tokens" },
            { "titles", title },
            { "formatversion", "2" },
            { "inprop", "protection|displatitle" },
            { "rvprop", "content|timestamp" },
            { "rvslots", "main" },
            { "type", "csrf" }
        };

        string queryString = string.Join("&",
            parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        string requestUrl = $"{ApiUrl}?{queryString}";

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Failed to retrieve article information. Status code: {response.StatusCode}");
            }

            string json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

            var page = apiResponse?.Query?.Pages?.FirstOrDefault();
            // the page wasn't found, for some reason -- need to handle this better, hehe but just return null for now!
            // caller responsible for checking for null and skipping the page
            if (page == null || page.PageId <= 0) return null;

            var revision = page.Revisions?.FirstOrDefault();
            string content = revision?.Slots?.GetValueOrDefault("main")?.Content ?? string.Empty;

            // try and create a new article
            Article.Article article = new Article.Article(page.Title, content)
            {
                PageId = page.PageId,
                ContentModel = page.ContentModel,
                PageLanguage = page.PageLanguage,
                PageLanguageHtmlCode = page.PageLanguageHtmlCode,
                PageLanguageDir = page.PageLanguageDir,
                LastTouched = page.Touched,
                LastRevisionId = page.LastRevId,
                Length = page.Length,
                Protections = page.Protection ?? new List<Protection>(),
                DisplayTitle = page.DisplayTitle,
                RevisionTimestamp = revision?.Timestamp
            };

            return article;
        }
        catch (HttpRequestException ex)
        {
            // http error such as 403 etc.
            // the caller is responsible for checking for null and skipping processing in this instance.
            Console.WriteLine($"Error fetching page: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            // something fucked happened with the JSON processing, return null, the caller is responsible
            // for skipping the page in this instance
            Console.WriteLine($"Error parsing response: {ex.Message}");
            return null;
        }
    }
}