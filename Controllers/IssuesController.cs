using Microsoft.AspNetCore.Mvc;
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

        await _redmineService.TakeIssueAsync(id);
        return RedirectToAction("Index");
    }
}