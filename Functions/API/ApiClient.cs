using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json;

namespace Functions.API;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Wiki _wiki;
    
    public string ApiUrl { get; private set; }
    
    public ApiClient(Wiki wiki)
    {
        _wiki = wiki;
        _httpClient = new HttpClient();
        
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
    /// Return a token for usage for doing a subsequent action (like logging in etc).
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<string?> GetTokenAsync(string type)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "action", "query " },
            { "meta", "tokens" },
            { "format", "json" },
            { "type", type }
        };
        
        var encodedContent = new FormUrlEncodedContent(parameters);
        string queryString = await encodedContent.ReadAsStringAsync();
        string requestUrl = $"{ApiUrl}?{queryString}";

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
}