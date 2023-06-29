using Microsoft.AspNetCore.Mvc;
using RedmineApp.Services;

namespace RedmineApp.Controllers;

public class LoginController: Controller
{
    public IActionResult Index()
    {
        string? apiKey = HttpContext.Session.GetString("apiKey");
        string? username = HttpContext.Session.GetString("username");
        string? password = HttpContext.Session.GetString("password");

        if (!string.IsNullOrEmpty(apiKey) && RedmineService.IsValidApiKey(apiKey))
        {
            return RedirectToAction("Index", "Issues");
        }
        else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            if (RedmineService.IsValidUserCredentials(username, password))
            {
                return RedirectToAction("Index", "Issues");
            }
        }

        return View();
    }

    [HttpPost]
    public IActionResult ValidateApiKey(string apiKey)
    {
        bool isValid = RedmineService.IsValidApiKey(apiKey);
        
        if (isValid)
        {
            HttpContext.Session.SetString("apiKey", apiKey);
            HttpContext.Session.Remove("username");
            HttpContext.Session.Remove("password");
        }

        return Json(new { isValid = isValid });
    }

    [HttpPost]
    public IActionResult IsValidUserCredentials(string username, string password)
    {
        bool isValid = RedmineService.IsValidUserCredentials(username, password);
        if (isValid)
        {
            HttpContext.Session.SetString("username", username);
            HttpContext.Session.SetString("password", password);
            HttpContext.Session.Remove("apiKey");
        }

        return Json(new { isValid = isValid });
    }
    
    
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Index");
    }
}