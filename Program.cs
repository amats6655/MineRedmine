using RedmineApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<RedmineService>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var apiKey = httpContextAccessor.HttpContext.Session.GetString("apiKey");
    var username = httpContextAccessor.HttpContext.Session.GetString("username");
    var password = httpContextAccessor.HttpContext.Session.GetString("password");

    if (!string.IsNullOrEmpty(apiKey))
    {
        return new RedmineService(apiKey);
    }
    else if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
    {
        return new RedmineService(username, password);
    }

    // Возвращаем простую заглушку, если сессия недействительна
    return new RedmineService();
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure the middleware that enables session state is added to the pipeline
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();