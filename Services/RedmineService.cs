using System.Collections.Specialized;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;
using Newtonsoft.Json;

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
            var orderedResult = result.OrderByDescending(issue => issue.UpdatedOn).ToList();
            return orderedResult;
        }

        public async Task<Issue> GetIssueAsync(int id)
        {
            var parameters = new NameValueCollection { { RedmineKeys.INCLUDE, RedmineKeys.JOURNALS } };
            return await _redmineManager.GetObjectAsync<Issue>(id.ToString(), parameters);
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
            
                var newStatus = GetIssueStatusesAsync(2).Result;
                issue.Status = newStatus;
                issue.AssignedTo = IdentifiableName.Create<IdentifiableName>(currentUser.Id);
                await _redmineManager.UpdateObjectAsync<Issue>(issueId.ToString(), issue);
            }

        private async Task<IssueStatus> GetIssueStatusesAsync(int id)
        {
            var issueStatuses = await _redmineManager.GetObjectsAsync<IssueStatus>(null);
            return issueStatuses.FirstOrDefault(status => status.Id == id);
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