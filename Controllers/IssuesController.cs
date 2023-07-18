using Microsoft.AspNetCore.Mvc;
using Redmine.Net.Api.Exceptions;
using RedmineApp.Services;

namespace RedmineApp.Controllers;

public class IssuesController : Controller
{
    private readonly RedmineService _redmineService;

    public IssuesController(RedmineService redmineService)
    {
        _redmineService = redmineService;
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
            return RedirectToAction("Index");
        }
        catch (RedmineException ex)

        {
            if(ex.Message.Contains("You are not authorized to access this page."))
            {
                ViewBag.ErrorMessage = "У вас нет прав на взятие этой задачи в работу";
                return View("Error");
            }
            else
            {
                ViewBag.ErrorMessage = "Произошла ошибка при попытке взять задачу в работу";
                return View("Error");
            }
        }

    }
}