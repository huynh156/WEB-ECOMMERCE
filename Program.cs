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
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
    options.Events.OnTicketReceived = async context =>
    {
        var email = context.Principal?.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
        if (!string.IsNullOrEmpty(email))
        {
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<FashionHubContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
            {
                if (!identity.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role))
                {
                    identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role));
                }
            }
        }
    };
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
