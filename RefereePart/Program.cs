using RefereePart.Data;
using RefereePart.Models;
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Text.Json.Serialization;
using System.Text.Json;
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

// Retrieve PostgreSQL credentials
string postgreUsername = builder.Configuration["UsernameReferee"];
string postgrePassword = builder.Configuration["PasswordReferee"];
const string _host = "172.18.2.4";
const string _database = "mydb";
const string _port = "5432";
string connectionString = $"Host={_host};Port={_port};Database={_database};Username={postgreUsername};Password={postgrePassword};";

// Add services to the container.
builder.Services.AddControllersWithViews()
	.AddJsonOptions(options =>
	{
	    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;    	
		options.JsonSerializerOptions.PropertyNameCaseInsensitive = false; // Make it case-sensitive
	});
builder.Services.AddHttpContextAccessor();

// Set the db context as PostgreSQL database
builder.Services.AddDbContext<DatabaserefereeContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
