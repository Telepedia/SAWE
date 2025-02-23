using System.Text;

namespace Functions.Article;

public class Article
{
    #region properties/fields
    protected string ArticleText { get; private set; }
    protected string OriginalArticleText { get; set; }
    public string Name { get; set; }
    private StringBuilder _editSummary = new();
    public string EditSummary => _editSummary.ToString();
    public bool Skipped { get; private set; }
    public string SkippedReason { get; private set; } = string.Empty;
    #endregion
    
    public Article(string name, string text)
    {
        OriginalArticleText = ArticleText = text;
        Name = name;
    }

    /// <summary>
    /// Appends to the summary field; alternatively if the summary field is empty, uses the passed string
    /// </summary>
    /// <param name="appendedText"></param>
    public void AppendToSummary(string appendedText)
    {
        string comma = ",";
        if (string.IsNullOrEmpty(appendedText.Trim())) return;

        if (_editSummary.Length > 0)
        {
            _editSummary.Append(comma + appendedText);
        }
        else
        {
            _editSummary.Append(appendedText);
        }
    }

    public void PerformFindAndReplace(FindAndReplaceEntry findAndReplaceEntry)
    {
        // TO BE IMPLEMENTED
    }

    public void PerformGeneralFixes()
    {
        // TO BE IMPLEMENTED
    }

}