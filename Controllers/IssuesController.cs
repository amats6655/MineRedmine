using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Redmine.Net.Api.Exceptions;
using Redmine.Net.Api.Types;
using RedmineApp.Services;

namespace RedmineApp.Controllers;

public class IssuesController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly RedmineService _redmineService;
    private readonly INotyfService _notyf;
    private readonly IMemoryCache _cache;
    private readonly Dictionary<string, string> _customFieldSettings;
    private readonly Dictionary<string, Dictionary<int, string>> _customFields;
    private readonly Dictionary<int, string> _buildings;

    public IssuesController(RedmineService redmineService, INotyfService notyf, IMemoryCache cache, IConfiguration configuration)
    {
        _configuration = configuration;
        _redmineService = redmineService;
        _notyf = notyf;
        _cache = cache;
        _buildings = _configuration.GetSection("CustomFields:Buildings").Get<Dictionary<int, string>>() 
                     ?? new Dictionary<int, string>();
    }
    
    public async Task<IActionResult> Index()
    {
        if (!_redmineService.IsSessionValid())
        {
            return RedirectToAction("Index", "Login");
        }
        
        // Попробовать получить список задач из кэша
        if (!_cache.TryGetValue("UserIssues", out List<Issue>? issues))
        {
            var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            issues = await _redmineService.GetIssuesAsync(clientIp);
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) // Кеширование на 1 минуту
            };
            _cache.Set("UserIssues", issues, cacheEntryOptions);
        }
        var issueBuildings = (
            from issue in issues 
            from cf in issue.CustomFields.ToList() 
            where cf.Name.Equals("Объект Фонда") 
            select cf).ToList();
        var uniqueBuildings = issueBuildings.Distinct().ToList();

        ViewData["IssueBuildings"] = issueBuildings;
        ViewData["UniqueBuildings"] = uniqueBuildings;
        ViewData["Buildings"] = _buildings;
        return View(issues);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!_redmineService.IsSessionValid())
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            var issue = await _redmineService.GetIssueAsync(id, clientIp);
            return View(issue);
        }
        catch (RedmineException ex)
        {
            if(ex.Message.Contains("Issue Not Found"))
            {
                _notyf.Error("Данная заявка не найдена");
                
            }
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    public async Task<IActionResult> TakeIssue(int id)
    {
        if (!_redmineService.IsSessionValid())
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            await _redmineService.TakeIssueAsync(id, clientIp);
            _notyf.Success("Взял в работу");
            return RedirectToAction("Index");
        }
        catch (RedmineException ex)

        {
            if(ex.Message.Contains("You are not authorized to access this page."))
            {
                _notyf.Error("Нет прав для изменения этой заявки");
                return RedirectToAction("Index");
            }

            if (ex.Message.Contains("Ты не можешь взять в работу"))
            {
                _notyf.Error(ex.Message);
                return RedirectToAction("Index");
            }

            _notyf.Error("Произошла ошибка при попытке взять задачу в работу");
            return RedirectToAction("Index");
        }
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