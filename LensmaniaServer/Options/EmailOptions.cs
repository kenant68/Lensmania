namespace LensmaniaServer.Options;

public class EmailOptions
{
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; }
    public bool UseStartTls { get; set; } = true;
    public string FromAddress { get; set; } = "";
    public string FromDisplayName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}