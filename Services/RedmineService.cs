using System.Collections.Specialized;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;

namespace RedmineApp.Services
{
    public class RedmineService
    {
        private readonly RedmineManager? _redmineManager;

        // Добавлен конструктор по умолчанию
        public RedmineService()
        {
            _redmineManager = null;
        }

        public RedmineService(string apiKey)
        {
            if (IsValidApiKey(apiKey))
            {
                _redmineManager = new RedmineManager("https://sd.talantiuspeh.ru", apiKey);
            }
        }

        public RedmineService(string username, string password)
        {
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
            var orderedEnumerable = result.OrderByDescending(issue => issue.UpdatedOn).ToList();
            return orderedEnumerable;
        }

        public async Task<Issue> GetIssueAsync(int id)
        {
            return await _redmineManager.GetObjectAsync<Issue>(id.ToString(), null);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var currentUser = await _redmineManager.GetCurrentUserAsync();
            return currentUser;
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