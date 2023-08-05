using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Redmine.Net.Api.Exceptions;
using Redmine.Net.Api.Types;
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
        var buildings = new Dictionary<int, string>()
        {
            {347, "Автодром"},
            {224, "Административные здания"},
            {226, "Айсберг"},
            {262, "Академия единоборств"},
            {481, "Альфа 4*"},
            {369, "Альфа Заповедный квартал"},
            {368, "Альфа Морской квартал"},
            {483, "Альфа Морской порт"},
            {371, "Альфа Парковый квартал"},
            {370, "Альфа Прибрежный квартал"},
            {227, "Парусная"},
            {256, "Бассейн на Воскресенской"},
            {228, "Гамма Сириус"},
            {351, "ГК Академии Единоборств"},
            {223, "Дельта"},
            {229, "Детский сад (Кампус)"},
            {482, "Детский сад (Континентальный)"},
            {231, "Детский сад (Фигурная)"},
            {232, "Детский сад (Общинная)"},
            {230, "Детсуий сад (Воскресенская)"},
            {233, "Кампус"},
            {234, "Комплекс трамплинов"},
            {261, "Концертный комплекс"},
            {235, "Корпус Спорт"},
            {254, "Корпус Школа"},
            {237, "Лицей на Воскресенской"},
            {236, "Лицей на Международной"},
            {353, "Медцентр Академии Единоборств"},
            {314, "Московский офис"},
            {239, "Олимпийский парк (площадь)"},
            {240, "Омега Сириус"},
            {241, "Отель Пульсар"},
            {238, "Парк Науки и Искусства"},
            {242, "Планетарий в Олимпийском парке"},
            {315, "Санкт-Петербургский офиса"},
            {255, "Санно-бобслейная трасса"},
            {243, "Сигма А"},
            {244, "Сигма Б"},
            {245, "Сигма В, Г"},
            {246, "Сириус-Арена"},
            {247, "Склады РЖД"},
            {248, "Спортивный парк (мыс Адлер)"},
            {249, "Тренировочная арена (ТАХШ)"},
            {250, "Тренировочный центр (ТЦФК)"},
            {251, "Хостел S Hostel"},
            {352, "ЦПС Академии Единоборств"},
            {252, "Шайба"},
            {253, "Школа №38"}
        };
        
        if (!_redmineService.IsSessionValid())
        {
            return RedirectToAction("Index", "Login");
        }
        var issues = await _redmineService.GetIssuesAsync();

        var uniqueBuildings = (
            from issue in issues 
            from cf in issue.CustomFields.ToList() 
            where cf.Name.Equals("Объект Фонда") 
            select cf).Distinct().ToList();
        var uniquePriority = (
            from issue in issues
            select issue.Priority.Name).Distinct().ToList();
        
        
        ViewData["UniqueBuildings"] = uniqueBuildings;
        ViewData["Buildings"] = buildings;
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
            else
            {
                _notyf.Error("Произошла ошибка при попытке взять задачу в работу");
                return RedirectToAction("Index");
            }
        }
    }
}