using System.Runtime.InteropServices;
using System.Text;

namespace Functions.SecureStorage;

/// <summary>
/// This class implements the ISecureStorage interface and is responsible for storing
/// username and passwords for the profiles in macOS Keychain. There is no need for us to encrypt
/// or otherwise hash passwords going into the Keychain -- we can let macOS handle deciding what and who
/// can access the Keychain and all of the security associated with this. It **should** be as secure as we
/// can possibly get, but we shall see.
/// </summary>
public class MacOSKeychain : ISecureStorage
{
    [DllImport("/System/Library/Frameworks/Security.framework/Security")]
    private static extern int SecKeychainAddGenericPassword(
        IntPtr keychain,
        uint serviceNameLength,
        string serviceName,
        uint accountNameLenght,
        string accountName,
        uint passwordLength,
        byte[] passwordData,
        out IntPtr itemRef
    );
    
    [DllImport("/System/Library/Frameworks/Security.framework/Security")]
    private static extern int SecKeychainFindGenericPassword(
        IntPtr keychainOrArray, 
        uint serviceNameLength, 
        string serviceName, 
        uint accountNameLength, 
        string accountName, 
        out uint passwordLength, 
        out IntPtr passwordData, 
        out IntPtr itemRef
    );
    
    [DllImport("/System/Library/Frameworks/Security.framework/Security")]
    private static extern int SecKeychainItemFreeContent(IntPtr attrList, IntPtr passwordData);
    
    [DllImport("/System/Library/Frameworks/Security.framework/Security")]
    private static extern int SecKeychainItemDelete(IntPtr itemRef);
    
    /// <summary>
    /// Adds a password to the macOS Keychain.
    /// </summary>
    public void AddPassword(string service, string account, string password)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        IntPtr itemRef;
        int result = SecKeychainAddGenericPassword(
            IntPtr.Zero,
            (uint)Encoding.UTF8.GetByteCount(service),
            service,
            (uint)Encoding.UTF8.GetByteCount(account),
            account,
            (uint)passwordBytes.Length,
            passwordBytes,
            out itemRef
        );
        if (result != 0)
        {
            throw new Exception("Error adding password to Keychain: " + result);
        }
    }
    
    /// <summary>
    /// Retrieves a password from the macOS Keychain.
    /// </summary>
    public string FindPassword(string service, string account)
    {
        uint passwordLength;
        IntPtr passwordData;
        IntPtr itemRef;
        int result = SecKeychainFindGenericPassword(
            IntPtr.Zero,
            (uint)Encoding.UTF8.GetByteCount(service),
            service,
            (uint)Encoding.UTF8.GetByteCount(account),
            account,
            out passwordLength,
            out passwordData,
            out itemRef
        );
        if (result != 0)
        {
            throw new Exception("Error finding password in Keychain: " + result);
        }
        byte[] passwordBytes = new byte[passwordLength];
        Marshal.Copy(passwordData, passwordBytes, 0, (int)passwordLength);
        // Free the memory allocated by SecKeychainFindGenericPassword; this is fucked but what can we do
        SecKeychainItemFreeContent(IntPtr.Zero, passwordData);
        return Encoding.UTF8.GetString(passwordBytes);
    }
    
    /// <summary>
    /// Deletes a password from the macOS Keychain.
    /// </summary>
    public void DeletePassword(string service, string account)
    {
        uint passwordLength;
        IntPtr passwordData;
        IntPtr itemRef;
        int result = SecKeychainFindGenericPassword(
            IntPtr.Zero,
            (uint)Encoding.UTF8.GetByteCount(service),
            service,
            (uint)Encoding.UTF8.GetByteCount(account),
            account,
            out passwordLength,
            out passwordData,
            out itemRef
        );
        if (result != 0)
        {
            throw new Exception("Error finding password for deletion in Keychain: " + result);
        }
        result = SecKeychainItemDelete(itemRef);
        if (result != 0)
        {
            throw new Exception("Error deleting password from Keychain: " + result);
        }
    }

}