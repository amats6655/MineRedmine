using System.Collections.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;
using Redmine.Net.Api.Exceptions;
using Serilog;

namespace RedmineApp.Services
{
    public class RedmineService
    {
        
        private readonly RedmineManager? _redmineManager;
        private readonly Serilog.ILogger _logger;
        private readonly IMemoryCache _cache;

        // Добавлен конструктор по умолчанию
        public RedmineService(IMemoryCache cache)
        {
            _cache = cache;
            _redmineManager = null;
        }

        public RedmineService(string apiKey, Serilog.ILogger logger, IMemoryCache cache) : this(cache)
        {
            _cache = cache;
            _logger = logger;
            if (IsValidApiKey(apiKey))
            {
                _redmineManager = new RedmineManager("https://sd.talantiuspeh.ru", apiKey);
            }
        }

        public RedmineService(string username, string password, Serilog.ILogger logger, IMemoryCache cache) : this(cache)
        {
            _cache = cache;
            _logger = logger;
            if (IsValidUserCredentials(username, password))
            {
                _redmineManager = new RedmineManager("https://sd.talantiuspeh.ru", username, password);
            }
        }

        public async Task<List<Issue>> GetIssuesAsync()
        {
            return await _cache.GetOrCreateAsync("UserIssues", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); // Кеширование на 10 минут
                var parameters = new NameValueCollection
                {
                    {RedmineKeys.ASSIGNED_TO_ID, "me"},
                    {RedmineKeys.STATUS_ID, "7|14|2"}
                };
                var result = await _redmineManager.GetObjectsAsync<Issue>(parameters);
                return result.OrderByDescending(issue => issue.UpdatedOn).ToList();
            });
        }

        public async Task<Issue> GetIssueAsync(int id, string? clientIp)
        {
            var parameters = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS } };
            var currentUser = await GetCurrentUserAsync();
            var log = _logger.ForContext("CurrentUser", new {Id = currentUser.Id, FirstName = currentUser.FirstName, LastName = currentUser.LastName}, destructureObjects: true)
                .ForContext("Issue", new {Id = id}, destructureObjects: true)
                .ForContext("ClientIp", clientIp, destructureObjects: true);
            try
            {
                log.Information($"Открыта задача");
                return await _redmineManager.GetObjectAsync<Issue>(id.ToString(), parameters);
            }
            catch (Exception)
            {
                log.Warning("Возникла ошибка при поиске заявки");
                throw new RedmineException("Issue Not Found");
            }

        }

        public async Task<User> GetCurrentUserAsync()
        {
            var currentUser = await _redmineManager.GetCurrentUserAsync();
            return currentUser;
        }

    public async Task TakeIssueAsync(int issueId, string? clientIp)
    {
        var issue = await _redmineManager.GetObjectAsync<Issue>(issueId.ToString(), null);
        var currentUser = await GetCurrentUserAsync();
        if (issue.Status.Id != 7 && issue.Status.Id != 2 && issue.Status.Id != 16 && issue.Status.Id != 15 && issue.Status.Id != 14)
        {
            var log = _logger.ForContext("CurrentUser", new {Id = currentUser.Id, FirstName = currentUser.FirstName, LastName = currentUser.LastName}, destructureObjects: true)
                             .ForContext("Issue", new {Id = issue.Id, Status = issue.Status.Name}, destructureObjects: true)
                             .ForContext("ClientIp", clientIp, destructureObjects: true);
            log.Warning($"Неудачная попытка взять в работу заявку в статусе {issue.Status.Name}");
            throw new RedmineException($"Ты не можешь взять в работу заявку в статусе {issue.Status.Name}");
        }

        issue.Status = IdentifiableName.Create<IssueStatus>(2);
        issue.AssignedTo = IdentifiableName.Create<IdentifiableName>(currentUser.Id);
        try
        {
            await _redmineManager.UpdateObjectAsync<Issue>(issueId.ToString(), issue);
            var log = _logger.ForContext("CurrentUser", new {Id = currentUser.Id, FirstName = currentUser.FirstName, LastName = currentUser.LastName}, destructureObjects: true)
                             .ForContext("Issue", new {Id = issue.Id, Status = issue.Status.Name}, destructureObjects: true)
                             .ForContext("ClientIp", clientIp, destructureObjects: true);
            log.Information($"Задача успешно обновлена");
        }
        catch (Exception ex)
        {
            var log = _logger.ForContext("CurrentUser", new {Id = currentUser.Id, FirstName = currentUser.FirstName, LastName = currentUser.LastName}, destructureObjects: true)
                             .ForContext("Issue", new {Id = issue.Id, Status = issue.Status}, destructureObjects: true)
                             .ForContext("Exception", ex, destructureObjects: true)
                             .ForContext("ClientIp", clientIp, destructureObjects: true);
            log.Error($"При обновлении задача возникла ошибка");
            throw new RedmineException("You are not authorized to access this page.");
        }
    }


        
        public static bool IsValidApiKey(string apiKey)
        {
            try
            {
                var redmineManager = new RedmineManager("https://sd.talantiuspeh.ru", apiKey);
                var currentUser = redmineManager.GetCurrentUser(); // Если ключ API недействителен, этот метод вызовет исключение
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidUserCredentials(string username, string password)
        {
            try
            {
                
                var redmineManager = new RedmineManager("https://sd.talantiuspeh.ru", username, password);
                var currentUser = redmineManager.GetCurrentUser();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Добавлен метод проверки действительности сессии
        public bool IsSessionValid()
        {
            return _redmineManager != null;
        }
    }
}