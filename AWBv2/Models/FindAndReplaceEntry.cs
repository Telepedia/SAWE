using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AWBv2.Models;

public class FindAndReplaceEntry : ReactiveObject
{
    /// <summary>
    /// This class represents a find and replace entry. Essentially, it is a single replacement that we should
    /// carry out, within a larger set of find and replacements.
    /// Note: "Minor", "After Fixes" has been removed which was present in AWB
    /// </summary>
    [Reactive] public string Find { get; set; } = string.Empty;
    [Reactive] public string Replace { get; set; } = string.Empty;
    [Reactive] public bool CaseSensitive { get; set; }
    [Reactive] public bool Regex { get; set; }
    [Reactive] public bool MultiLine { get; set; }
    [Reactive] public bool SingleLine { get; set; }
    [Reactive] public bool Enabled { get; set; }
    [Reactive] public string Comment { get; set; } = string.Empty;
}