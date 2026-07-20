using FashionHubWeb;
using FashionHubWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using FashionHubWeb.Helper;
using FashionHubWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FashionHubContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=.;Database=FashionHub;Trusted_Connection=True;TrustServerCertificate=True;"));

// Register Payment Services
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddSingleton(x => new PaypalClient(
    builder.Configuration["PaypalOptions:ClientId"] ?? "AdCmK0EHBRwDoaOdLNzKTAddEAiOMVnaHMs876hH_zihAiXobEWcYWWWUNke1sQxL0ZHAKYJS9rulC-R",
    builder.Configuration["PaypalOptions:ClientSecret"] ?? "EKtsKT1jiSP18X4t_NInsPq_krGbrbfCeUARMQH2f3i70bHoMP64Vswc9-LBmonTJct3NJdzB3awvN-E",
    builder.Configuration["PaypalOptions:Mode"] ?? "sandbox"
));

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
});

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FashionHubContext>();
    DataSeeder.Initialize(context);
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
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
