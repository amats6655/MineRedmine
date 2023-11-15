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
    private readonly Dictionary<string, List<IssueCustomField>> _customFieldData;
    private readonly Dictionary<string, Dictionary<int, string>> _customFieldNames;
    private readonly Dictionary<int, string> _buildings;

    public IssuesController(RedmineService redmineService, INotyfService notyf, IMemoryCache cache, IConfiguration configuration)
    {
        _configuration = configuration;
        _redmineService = redmineService;
        _notyf = notyf;
        _cache = cache;
        _customFieldNames =
            _configuration.GetSection("CustomFieldNames").Get<Dictionary<string, Dictionary<int, string>>>() ??
            new Dictionary<string, Dictionary<int, string>>();
        _customFieldSettings = _configuration.GetSection("CustomFieldsSettings").Get<Dictionary<string, string>>() 
                               ?? new Dictionary<string, string>();
        _customFieldData = new Dictionary<string, List<IssueCustomField>>();
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

        foreach (var fieldSetting in _customFieldSettings)
        {
            var issueCustomField = (
                from issue in issues
                from cf in issue.CustomFields
                where cf != null && cf.Name.Equals(fieldSetting.Value)
                select cf).Distinct().ToList();
            _customFieldData[fieldSetting.Key] = issueCustomField ?? new List<IssueCustomField>();
        }
        foreach (var field in _customFieldData.Keys)
        {
            ViewData[field] = _customFieldData[field];
        }
        
        ViewData["CustomFieldsSettings"] = _customFieldSettings;
        ViewData["CustomFieldNames"] = _customFieldNames;
        
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
    public async Task<IActionResult> AddComment(int id, string comment, bool privateNotes, IFormFile? file)
    {
        
        if (!_redmineService.IsSessionValid())
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var clientIp = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            byte[] fileData = null;
            string fileName = null;
            string fileContentType = null;
            if (file != null)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    fileData = ms.ToArray();
                    fileName = file.FileName;
                    fileContentType = file.ContentType;
                }
                await _redmineService.AddCommentAsync(id, clientIp, comment, privateNotes, fileData, fileName, fileContentType);
                return RedirectToAction("Edit", new {id = id});
            }
            await _redmineService.AddCommentAsync(id, clientIp, comment, privateNotes);
            return RedirectToAction("Edit", new {id = id});
        }
        catch
        {
            return RedirectToAction("Index");
        }
    }
}