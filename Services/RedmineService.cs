using System.Collections.Specialized;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;
using Redmine.Net.Api.Exceptions;

namespace RedmineApp.Services
{
    public class RedmineService
    {
        
        private readonly RedmineManager? _redmineManager;
        private readonly ILogger<RedmineService> _logger;

        // Добавлен конструктор по умолчанию
        public RedmineService()
        {
            _redmineManager = null;
        }

        public RedmineService(string apiKey, ILogger<RedmineService> logger)
        {
            _logger = logger;
            if (IsValidApiKey(apiKey))
            {
                _redmineManager = new RedmineManager("https://sd.talantiuspeh.ru", apiKey);
            }
        }

        public RedmineService(string username, string password, ILogger<RedmineService> logger)
        {
            _logger = logger;
            if (IsValidUserCredentials(username, password))
            {
                _redmineManager = new RedmineManager("https://sd.talantiuspeh.ru", username, password);
            }
        }

        public async Task<List<Issue>> GetIssuesAsync()
        {
            var parameters = new NameValueCollection
            {
                {RedmineKeys.ASSIGNED_TO_ID, "me"},
                {RedmineKeys.STATUS_ID, "7|14|2"}
            };
            var result = await _redmineManager.GetObjectsAsync<Issue>(parameters);
            var orderedResult = result.OrderByDescending(issue => issue.UpdatedOn).ToList();
            return orderedResult;
        }

        public async Task<Issue> GetIssueAsync(int id)
        {
            var parameters = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS } };
            try
            {
                return await _redmineManager.GetObjectAsync<Issue>(id.ToString(), parameters);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"При поиске задачи возникла ошибка: \n {ex.ToString()}");
                throw new RedmineException("Issue Not Found");
            }

        }

        public async Task<User> GetCurrentUserAsync()
        {
            var currentUser = await _redmineManager.GetCurrentUserAsync();
            return currentUser;
        }

        public async Task TakeIssueAsync(int issueId)
            {
            var issue = await _redmineManager.GetObjectAsync<Issue>(issueId.ToString(), null);
            var currentUser = await GetCurrentUserAsync();
            if (issue.Status.Id != 7 && issue.Status.Id != 2 && issue.Status.Id != 16 && issue.Status.Id != 15 && issue.Status.Id != 14)
            {
                _logger.LogWarning($"Неудачная попытка взять в работу заявку в статусе {issue.Status.Name}. Заявка {issue.Id}, Пользователь {currentUser.FirstName} {currentUser.LastName}");
                throw new RedmineException($"Ты не можешь взять в работу заявку в статусе {issue.Status.Name}");
            }
            
            issue.Status = IdentifiableName.Create<IssueStatus>(2);
            issue.AssignedTo = IdentifiableName.Create<IdentifiableName>(currentUser.Id);
                try
                {
                    await _redmineManager.UpdateObjectAsync<Issue>(issueId.ToString(), issue);
                    _logger.LogInformation($"Задача успешно обновлена. Пользователь - {currentUser.FirstName} {currentUser.LastName}, Задача - {issue.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"При обновлении задача возникла ошибка. Пользователь - {currentUser.FirstName} {currentUser.LastName}, Задача - {issue.Id} \n {ex}");
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