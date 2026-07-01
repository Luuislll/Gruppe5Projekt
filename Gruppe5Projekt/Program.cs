using Google.GenAI;
using Gruppe5Projekt.Data;
using Gruppe5Projekt.Models;
using Gruppe5Projekt.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseAzureSql(connectionString));

// ASP.NET Core Identity: Benutzer- und Rollenverwaltung als Service.
// Es wird bewusst nur der Identity-Dienst registriert (kein Default-UI von
// Identity); Login/Logout laufen über den eigenen AccountController.
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie-Pfade auf die eigenen Account-Endpunkte umbiegen.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

// Azure Blob Storage (Connection String aus den User Secrets).
builder.Services.AddAzureClients(clients =>
{
    clients.AddBlobServiceClient(builder.Configuration.GetConnectionString("AzureBlobStorage"));
});
builder.Services.AddScoped<KapitelMaterialService>();

// Gemini-Client aus dem SDK (API-Key aus den User Secrets unter "Gemini:ApiKey").
var geminiApiKey = builder.Configuration["Gemini:ApiKey"];
builder.Services.AddSingleton(new Client(apiKey: geminiApiKey));
builder.Services.AddScoped<GeminiQuestionService>();

var app = builder.Build();

// Migrationen anwenden und Demodaten anlegen (nur Development).
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DemoDataSeeder.SeedAsync(db);

    // Rollen (Admin / User) und einen Standard-Admin anlegen.
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    await IdentitySeeder.SeedAsync(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();