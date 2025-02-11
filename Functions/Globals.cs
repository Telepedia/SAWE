namespace Functions;

public static class Globals
{
    public static bool IsWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
    public static bool IsLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
    public static bool IsMac = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);

    /// <summary>
    /// Return the data directory where we can store stuff for the current user
    /// May need to adjust these paths later on if they are not suitable
    /// </summary>
    /// <returns></returns>
    public static string GetDataDirPath()
    {
        string basePath;

        if (IsWindows)
        {
            basePath = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "AWBv2");
        }
        else if (IsMac)
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AWBv2" );
        }
        else
        {
            // the user is on Linux, or we fucked up 
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            basePath = Path.Combine(homePath, ".local", "share", "AWBv2");
        }

        // ensure that the directory exists. This is a safe operation and will take no action
        // and return no error if the directory already exists, so we can call it at every startup.
        Directory.CreateDirectory(basePath);
        
        return basePath;
        
    }
}