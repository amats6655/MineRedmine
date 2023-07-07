using Microsoft.AspNetCore.Mvc;
using RedmineApp.Services;

namespace RedmineApp.Controllers;

public class LoginController: Controller
{
    public IActionResult Index()
    {
        var apiKey = HttpContext.Session.GetString("apiKey");
        var username = HttpContext.Session.GetString("username");
        var password = HttpContext.Session.GetString("password");

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
        var isValid = RedmineService.IsValidApiKey(apiKey);
        
        if (isValid)
        {
            HttpContext.Session.SetString("apiKey", apiKey);
            HttpContext.Session.Remove("username");
            HttpContext.Session.Remove("password");
        }

        return Json(new { isValid });
    }

    [HttpPost]
    public IActionResult IsValidUserCredentials(string username, string password)
    {
        var isValid = RedmineService.IsValidUserCredentials(username, password);
        if (isValid)
        {
            HttpContext.Session.SetString("username", username);
            HttpContext.Session.SetString("password", password);
            HttpContext.Session.Remove("apiKey");
        }

        return Json(new { isValid });
    }
    
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Index");
    }
}