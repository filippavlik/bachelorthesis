using LoginPart.Data;
using LoginPart.Identity;
using LoginPart.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SendGrid.Extensions.DependencyInjection;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Net;
//TOP LEVEL STATEMENTS
// Get access information for AzureKey Vault 
string vaultUri = File.ReadAllText("/run/secrets/AzureVaultURI").Trim();
string tenantId = File.ReadAllText("/run/secrets/AzureTenantId").Trim();
string clientId = File.ReadAllText("/run/secrets/AzureClientId").Trim();
string clientSecret = File.ReadAllText("/run/secrets/AzureClientSecret").Trim();

var builder = WebApplication.CreateBuilder(args);

//Add support for AzureKeyVault
builder.Configuration.AddAzureKeyVault(
    new Uri(vaultUri),
    new ClientSecretCredential(
        tenantId: tenantId,
        clientId: clientId,
        clientSecret: clientSecret
        ));

string sendGridKey = builder.Configuration["SendGridKey"];

// Retrieve PostgreSQL credentials
string postgreUsername = builder.Configuration["Username"];
string postgrePassword = builder.Configuration["Password"];
const string _host = "172.18.0.3";
const string _database = "mydb";
const string _port = "5432";
string connectionString = $"Host={_host};Port={_port};Database={_database};Username={postgreUsername};Password={postgrePassword};";

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 8080); // Listens on both IPv4 and IPv6
});
//Configure SendGrid Api key
builder.Services.AddSendGrid(options =>
    options.ApiKey = sendGridKey!
);

// Register IJwtTokenService and its implementation
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
//Add service emailSender , SendGrid
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddAuthorizationBuilder();


// Set the db context as PostgreSQL database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add all requirments for user data given in process of registration
builder.Services.AddIdentity<Users, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = true;
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Enable lockout for failed access attempts
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Lockout duration
        options.Lockout.MaxFailedAccessAttempts = 5; // Number of attempts before lockout
        options.Lockout.AllowedForNewUsers = true; // Enable for new users
	
	// Configure token expiration to be 15 minutes
        options.Tokens.EmailConfirmationTokenProvider = "Default";	

    }
)
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// Suppress EF Core SQL command logging
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
// Minimize Microsoft and System logging
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);



var app = builder.Build();

// Seed roles on application startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRoles.Initialize(services, roleManager);  // Seed roles
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
