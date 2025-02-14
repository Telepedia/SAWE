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
}