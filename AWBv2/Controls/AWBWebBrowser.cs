using Avalonia.Controls;
using System;
using Xilium.CefGlue.Avalonia;

namespace AWBv2.Controls;

public class AWBWebBrowser : UserControl, IDisposable {

    private readonly AvaloniaCefBrowser _browser;
    
    public AWBWebBrowser()
    {
        _browser = new AvaloniaCefBrowser();
        this.Content = _browser;
    }

    /// <summary>
    /// Navigate to a supplied URL
    /// </summary>
    /// <param name="url">The URL to navigate to; will often be a diff page</param>
    public void Navigate(string url) => _browser.Address = url;

    /// <summary>
    /// Dispose of this resource
    /// </summary>
    public void Dispose() => _browser?.Dispose();
}