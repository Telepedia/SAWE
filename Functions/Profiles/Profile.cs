namespace AWBv2.Models;

public class Profile
{
    public int ID { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public byte[] EncryptedPassword { get; set; }
    public byte[] IV { get; set; } 
    
    public string Wiki { get; set; }
}
