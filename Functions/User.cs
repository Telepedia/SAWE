namespace Functions;

public class User
{
    public string Username { get; private set; }
    public bool IsBot { get; private set; }
    public bool IsBlocked { get; private set; }
    public List<string> Groups { get; private set; }
    public List<string> Rights { get; private set; }

    public User(string username, bool isBot, bool isBlocked, List<string> groups, List<string> rights)
    {
        this.Username = username;
        this.IsBot = isBot;
        this.IsBlocked = isBlocked;
        this.Groups = groups ?? new List<string>();
        this.Rights = rights ?? new List<string>();
    }
}