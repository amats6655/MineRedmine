﻿using System.Collections.Specialized;
using Microsoft.Extensions.Caching.Memory;
using Redmine.Net.Api;
using Redmine.Net.Api.Async;
using Redmine.Net.Api.Types;
using Redmine.Net.Api.Exceptions;
using Serilog;

namespace RedmineApp.Services;
public class RedmineService
{
    private readonly string _redmineServerUrl;
    private readonly RedmineManager? _redmineManager;
    private readonly Serilog.ILogger _logger;
    private readonly IMemoryCache _cache;

    // Добавлен конструктор по умолчанию
    public RedmineService(IConfiguration configuration, IMemoryCache cache)
    {
        _redmineServerUrl = configuration["RedmineSettings:ServerUrl"]!;
        _cache = cache;
        _redmineManager = null;
    }

    public RedmineService(string apiKey, Serilog.ILogger logger, IMemoryCache cache, IConfiguration configuration) : this(configuration, cache)
    {
        _cache = cache;
        _logger = logger;
        _redmineManager = InitializeRedmineManager(apiKey);
    }

    public RedmineService(string username, string password, Serilog.ILogger logger, IMemoryCache cache, IConfiguration configuration) : this(configuration, cache)
    {
        _cache = cache;
        _logger = logger;
        _redmineManager = InitializeRedmineManager(username, password);
    }
    
    private RedmineManager? InitializeRedmineManager(string apiKey)
    {
        return IsValidApiKey(apiKey) ? new RedmineManager(_redmineServerUrl, apiKey) : null;
    }

    private RedmineManager? InitializeRedmineManager(string username, string password)
    {
        return IsValidUserCredentials(username, password) ? new RedmineManager(_redmineServerUrl, username, password) : null;
    }

    public async Task<List<Issue>?> GetIssuesAsync(string? clientIp)
    {
        var currentUser = await GetCurrentUserAsync();
        var log = _logger.ForContext("CurrentUser",
                new { Id = currentUser.Id, FirstName = currentUser.FirstName, LastName = currentUser.LastName },
                destructureObjects: true)
            .ForContext("Issue", new { Id = 0 }, destructureObjects: true)
            .ForContext("ClientIp", clientIp, destructureObjects: true);
        
        return await _cache.GetOrCreateAsync("UserIssues", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10); // Кеширование на 10 минут
            var parameters = new NameValueCollection
            {
                {RedmineKeys.ASSIGNED_TO_ID, "me"},
                {RedmineKeys.STATUS_ID, "7|14|2"}
            };
            var result = await _redmineManager.GetObjectsAsync<Issue>(parameters);
            log.Information("Получен список заявок");
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

    private async Task<User> GetCurrentUserAsync()
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

    public async Task AddCommentAsync(int issueId, string clientIp, string comment, bool privateNotes, byte[] fileData = null, string? fileName = null, string fileContentType = null)
    {
        var issue = await _redmineManager.GetObjectAsync<Issue>(issueId.ToString(), null);
        var currentUser = await GetCurrentUserAsync();
        if (!string.IsNullOrEmpty(comment))
        {
            issue.Notes = comment;
            issue.PrivateNotes = privateNotes;
            if (fileData != null && fileName != null && fileContentType != null)
            {
                var attachment = await _redmineManager.UploadFileAsync(fileData);
                attachment.FileName = fileName;
                attachment.Description = comment;
                attachment.ContentType = fileContentType;
                IList<Upload> attachments = new List<Upload>{attachment};
                issue.Uploads = attachments;
            }
            try
            {
                await _redmineManager.UpdateObjectAsync(issueId.ToString(), issue);
                var log = _logger.ForContext("CurrentUser", new {Id = currentUser.Id, FirstName = currentUser.FirstName, LastName = currentUser.LastName}, destructureObjects: true)
                    .ForContext("Issue", new {Id = issue.Id, Status = issue.Status.Name}, destructureObjects: true)
                    .ForContext("ClientIp", clientIp, destructureObjects: true);
                log.Information($"Добавлен комментарий");
            }
            catch (Exception ex)
            {
                var log = _logger.ForContext("CurrentUser", new {Id = currentUser.Id, FirstName = currentUser.FirstName, LastName = currentUser.LastName}, destructureObjects: true)
                    .ForContext("Issue", new {Id = issue.Id, Status = issue.Status}, destructureObjects: true)
                    .ForContext("Exception", ex, destructureObjects: true)
                    .ForContext("ClientIp", clientIp, destructureObjects: true);
                log.Error(ex.ToString());
            }
        }
    }
    
    public bool IsValidApiKey(string apiKey)
    {
        try
        {
            var redmineManager = new RedmineManager(_redmineServerUrl, apiKey);
            var currentUser = redmineManager.GetCurrentUser(); // Если ключ API недействителен, этот метод вызовет исключение
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidUserCredentials(string username, string password)
    {
        try
        {
            
            var redmineManager = new RedmineManager(_redmineServerUrl, username, password);
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