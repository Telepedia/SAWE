using System.Net;
using System.Net.Security;
using System.Security.Authentication;

namespace Functions.API;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Wiki _wiki;
    public string ApiUrl { get; set; }
    
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
    public async Task DetermineScriptPathAsync()
    {
        // Check /w/ path
        string testUrlW = $"{_wiki.Url}/w/{_wiki.ApiPHP}?action=query&meta=siteinfo&format=json";
        HttpResponseMessage responseW = await _httpClient.GetAsync(testUrlW);

        if (responseW.IsSuccessStatusCode)
        {
            _wiki.ScriptPath = "/w/";
            ApiUrl = $"{_wiki.Url}/w/{_wiki.ApiPHP}";
            return;
        }

        // Check root path
        string testUrlRoot = $"{_wiki.Url}/{_wiki.ApiPHP}?action=query&meta=siteinfo&format=json";
        HttpResponseMessage responseRoot = await _httpClient.GetAsync(testUrlRoot);

        if (responseRoot.IsSuccessStatusCode)
        {
            _wiki.ScriptPath = "/";
            ApiUrl = $"{_wiki.Url}/{_wiki.ApiPHP}";
            return;
        }

        throw new InvalidOperationException("Unable to determine script path. AWBv2 only supports script paths of /w/ or / type.");
    }
    
    
    
    
}