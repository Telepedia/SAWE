namespace Functions;

public class FindAndReplaceEntry
{
    /// <summary>
    /// This class represents a find and replace entry. Essentially, it is a single replacement that we should
    /// carry out, within a larger set of find and replacements.
    /// Note: "Minor", "After Fixes" has been removed which was present in AWB
    /// </summary>
    public string Find { get; set; } = string.Empty;
    public string Replace { get; set; } = string.Empty;
    public bool CaseSensitive { get; set; }
    public bool Regex { get; set; }
    public bool Multiline { get; set; }
    public bool Singleline { get; set; }
    public bool Enabled { get; set; }
    public string Comment { get; set; } = string.Empty;
}