using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Redmine.Net.Api.Exceptions;
using RedmineApp.Services;

namespace RedmineApp.Controllers;

public class IssuesController : Controller
{
    private readonly RedmineService _redmineService;
    private readonly INotyfService _notyf;

    public IssuesController(RedmineService redmineService, INotyfService notyf)
    {
        _redmineService = redmineService;
        _notyf = notyf;
    }

    public async Task<IActionResult> Index()
    {
        if (!_redmineService.IsSessionValid())
        {
            return RedirectToAction("Index", "Login");
        }

        var issues = await _redmineService.GetIssuesAsync();
        return View(issues);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!_redmineService.IsSessionValid())
        {
            return RedirectToAction("Index", "Login");
        }

        var issue = await _redmineService.GetIssueAsync(id);
        return View(issue);
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
            await _redmineService.TakeIssueAsync(id);
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
            else
            {
                _notyf.Error("Произошла ошибка при попытке взять задачу в работу");
                return RedirectToAction("Index");
            }
        }
    }
}