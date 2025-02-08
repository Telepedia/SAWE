namespace Functions;

public enum ProjectEnum
{
    wikipedia,
    wiktionary,
    wikidata,
    wikisource,
    wikiquote,
    wikiversity,
    wikivoyage,
    wikibooks,
    wikinews,
    species,
    commons,
    meta,
    mediawiki,
    incubator,
    telepedia,
    miraheze,
    wikigg,
    fandom,
    custom
}

public static partial class Variables
{
    /// <summary>
    /// Provides access to the en namespace keys e.g. Category:
    /// </summary>
    public static readonly Dictionary<int, string> CanonicalNamespaces = new Dictionary<int, string>(20);

    /// <summary>
    /// Whether the wiki capitalizes first letter of page names (usually yes, e.g. English Wikipedia) or not (e.g. Wiktionary)
    /// This is based on the value of $wgCapitalLinks
    /// No need to provide getters and setters for this since we don't need to private the setter like the others etc.
    /// </summary>
    public static bool CapitalizeFirstLetter = true;

    /// <summary>
    /// index.php 
    /// </summary>
    public static string IndexPHP { get; private set; } = "index.php";

    /// <summary>
    /// api.php 
    /// </summary>
    public static string ApiPHP { get; private set; } = "api.php";

    /// <summary>
    /// The edit summary used when fixing typos
    /// Should not start with spaces or commas. Must end with a space
    /// </summary>
    public static string TypoSummaryTag { get; private set; } = "typos fixed: ";

    /// <summary>
    /// Localized version of " using " for edit summary tag
    /// Does not need spaces at start or end
    /// </summary>
    private static string mSummaryTag = "using ";

    /// <summary>
    /// Protocol, HTTP or HTTPS?
    /// </summary>
    public static string Protocol { get; private set; } = "https://";

    /// <summary>
    /// Whether user notifications from Echo are available on the wiki; note that this doesn't support Fandom
    /// notifications, but possibly in the future this can be added, I suppose if this project ever gets off the ground
    /// properly
    /// </summary>
    public static bool NotificationsEnabled = true;

    /// <summary>
    /// Whether the wiki uses Unicode (uca-) sorting for category sort keys, i.e. the wgCategoryCollation value is a uca-type
    /// </summary>
    public static bool UnicodeCategoryCollation = false;

    public static string WPAWB { get; private set; } = "[https://github.com/OAuthority/AWBv2 AWBv2]";
    
    /// <summary>
    /// The username to use for logging in
    /// @TODO: potentially switch from using this to using OAuth in future.
    /// </summary>
    public static string HttpAuthUsername { get; set; }

    /// <summary>
    /// The password used for logging in
    /// @TODO: potentially switch from using this to using OAuth in future.
    /// </summary>
    public static string HttpAuthPassword { get; set; }
    
    /// <summary>
    /// localized names of months
    /// </summary>
    public static string[] MonthNames;
    
    /// <summary>
    /// Month names for when the user is using English; since this is the most common.
    /// </summary>
    public static readonly string[] ENLangMonthNames =
    {
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December"
    };
    
    /// <summary>
    /// Gets a name of the project, e.g. "telepedia".
    /// </summary>
    public static ProjectEnum Project { get; private set; }
    
    /// <summary>
    /// Gets the language code, e.g. "en".
    /// </summary>
    public static string LangCode { get; internal set; }
    
    /// <summary>
    /// Gets a URL of the site, e.g. "https://meta.telepedia.net".
    /// </summary>
    public static string URL = "https://meta.telepedia.net";
    
    static Variables()
    {
        // setup namespaces -- probably cut alot of these down in the future to remove all of the wikipedia 
        // specific stuff. The vision I have for this is that it isn't overly Wikipedia specific, but I guess,
        // there would be no harm in this at present. 
        CanonicalNamespaces[-2] = "Media:";
        CanonicalNamespaces[-1] = "Special:";
        CanonicalNamespaces[1] = "Talk:";
        CanonicalNamespaces[2] = "User:";
        CanonicalNamespaces[3] = "User talk:";
        CanonicalNamespaces[4] = "Project:";
        CanonicalNamespaces[5] = "Project talk:";
        CanonicalNamespaces[6] = "File:";
        CanonicalNamespaces[7] = "File talk:";
        CanonicalNamespaces[8] = "MediaWiki:";
        CanonicalNamespaces[9] = "MediaWiki talk:";
        CanonicalNamespaces[10] = "Template:";
        CanonicalNamespaces[11] = "Template talk:";
        CanonicalNamespaces[12] = "Help:";
        CanonicalNamespaces[13] = "Help talk:";
        CanonicalNamespaces[14] = "Category:";
        CanonicalNamespaces[15] = "Category talk:";
        CanonicalNamespaces[118] = "Draft:";
        CanonicalNamespaces[119] = "Draft talk:";
        CanonicalNamespaces[126] = "MOS:";
        CanonicalNamespaces[127] = "MOS talk:";
        CanonicalNamespaces[710] = "TimedText:";
        CanonicalNamespaces[711] = "TimedText talk:";
        CanonicalNamespaces[828] = "Module:";
        CanonicalNamespaces[829] = "Module talk:";
    }
    
}