using Elsa;
using Elsa.Activities.Http.Options;
using Elsa.Events;
using Elsa.Extensions;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;

using Elsa.Activities.Http.Extensions;
using ElsaDemo2.WorkFlows;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddElsaCore(options =>
    options
        .UseEntityFrameworkPersistence(ef => ef.UseSqlite("Data Source=elsa.db;"), true)
        .AddHttpActivities()
        .AddWorkflow<HelloWorld>())
    .AddElsaApiEndpoints();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//ELSA
app.UseHttpActivities();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();