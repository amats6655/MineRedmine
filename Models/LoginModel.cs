namespace RedmineApp.Models;

public class LoginModel
{
    public string? ApiKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool AuthenticateUsingApiKey { get; set; }
}