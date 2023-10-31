using AspNetCoreHero.ToastNotification;
using Microsoft.Extensions.Caching.Memory;
using RedmineApp.Services;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
    .Enrich.FromLogContext()
    .WriteTo.File(new CompactJsonFormatter(),
        path: Path.Combine("logs", "log-.json"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 3 * 30)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(2); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<RedmineService>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var apiKey = httpContextAccessor.HttpContext?.Session.GetString("apiKey");
    var username = httpContextAccessor.HttpContext?.Session.GetString("username");
    var password = httpContextAccessor.HttpContext?.Session.GetString("password");
    var logger = Log.Logger;
    var cache = sp.GetRequiredService<IMemoryCache>();
    

    if (!string.IsNullOrEmpty(apiKey))
    {
        return new RedmineService(apiKey, logger, cache);
    }
    else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
    {
        return new RedmineService(username, password, logger, cache);
    }

    // Возвращаем простую заглушку, если сессия недействительна
    return new RedmineService(cache);
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddNotyf(config => { config.DurationInSeconds = 5; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });


var app = builder.Build();

// Ensure the middleware that enables session state is added to the pipeline
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseForwardedHeaders();
app.UseRouting();

app.UseAuthorization();

app.Run();