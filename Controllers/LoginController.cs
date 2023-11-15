using Microsoft.AspNetCore.Mvc;
using RedmineApp.Services;

namespace RedmineApp.Controllers;

public class LoginController: Controller
{
    private readonly RedmineService _redmineService;
    
    public LoginController(RedmineService redmineService)
    {
        _redmineService = redmineService;
    }
    
    public IActionResult Index()
    {
        var apiKey = HttpContext.Session.GetString("apiKey");
        var username = HttpContext.Session.GetString("username");
        var password = HttpContext.Session.GetString("password");

        if (!string.IsNullOrEmpty(apiKey) && _redmineService.IsValidApiKey(apiKey))
        {
            return RedirectToAction("Index", "Issues");
        }
        else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            if (_redmineService.IsValidUserCredentials(username, password))
            {
                return RedirectToAction("Index", "Issues");
            }
        }

        return View();
    }
    


}