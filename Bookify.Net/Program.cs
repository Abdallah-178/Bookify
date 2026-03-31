using Bookify.Net.Core.Mappeing;
using Bookify.Net.Helpers;
using Bookify.Net.Seeds;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

builder.Services.Configure<SecurityStampValidatorOptions>(option => option.ValidationInterval = TimeSpan.Zero);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

////////////////////////////////////////////////
builder.Services.Configure<IdentityOptions>(options =>
{
    /// ForPassword
    //options.Password.RequireDigit = true;
    //options.Password.RequireLowercase = true;
    //options.Password.RequireNonAlphanumeric = true;
    //options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    //options.Password.RequiredUniqueChars = 1;

    /// ForUserName And Email 
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; /// ForUserName Without -._@+
    options.User.RequireUniqueEmail = true;


    // Default Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;







});

builder.Services.AddTransient<IImageService, ImageService>();



builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));

////////////////////////////////////////////////
///                 For Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.Name = "YourAppCookieName";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";
    // ReturnUrlParameter requires 
    //using Microsoft.AspNetCore.Authentication.Cookies;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});
////////////////////////////////////////////////
builder.Services.AddWhatsAppApiClient(builder.Configuration);

builder.Services.AddDataProtection().SetApplicationName(nameof(Bookify));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true) // Confirm Email
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

////////////////////////////////////////////////
builder.Services.AddControllersWithViews();
builder.Services.AddExpressiveAnnotations();

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection(nameof(CloudinarySettings)));

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();




builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHangfireDashboard("/hangfire");
app.UseHttpsRedirection();
app.UseRouting();
////////////////////////////////////////////////

app.UseAuthentication();
app.UseAuthorization();


var ScopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var Scope = ScopeFactory.CreateScope();

var RoleManager = Scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var UserManager = Scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedRolesAsync(RoleManager);

await DefaultUsers.SeedAdminUserAsync(UserManager);

////////////////////////////////////////////////
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
