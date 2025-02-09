namespace Functions.SecureStorage;

public interface ISecureStorage
{
    /// <summary>
    /// Add an account/password to the storage
    /// </summary>
    /// <param name="service"></param>
    /// <param name="account"></param>
    /// <param name="password"></param>
    void AddPassword(string service, string account, string password);
    
    /// <summary>
    /// Find the password for a specific account from the storage solution
    /// </summary>
    /// <param name="service"></param>
    /// <param name="account"></param>
    /// <returns></returns>
    string FindPassword(string service, string account);
    
    /// <summary>
    /// Delete the password/account from the secure storage
    /// </summary>
    /// <param name="service"></param>
    /// <param name="account"></param>
    void DeletePassword(string service, string account);
}