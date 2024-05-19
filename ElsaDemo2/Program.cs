using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;

using ElsaDemo2.WorkFlows;
using Elsa.Persistence.EntityFrameworkCore.DbContexts;
using ElsaDemo2.Controllers;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<ElsaContext>(options =>
//{
//    options.UseSqlite("Data Source=elsa.db;");
//});

//builder.Services.AddDbContext<ElsaContext>(options =>
//{
//    options.UseSqlite("Data Source=elsa.db;");
//});

builder.Services.AddDbContextFactory<ElsaContext>(options =>
{
    options.UseSqlite("Data Source=elsa.db;");
});

builder.Services.AddScoped<BinApprovalController>();


builder.Services.AddElsaCore(options =>
    options
        .UseEntityFrameworkPersistence(ef => ef.UseSqlite("Data Source=elsa.db;"), true)
        .AddHttpActivities()
        .AddWorkflow<HelloWorld>()
    .AddWorkflow<TestingWorkflow>())
    .AddElsaApiEndpoints();

builder.Services.AddControllersWithViews();

builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//ELSA
app.UseHttpActivities();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();