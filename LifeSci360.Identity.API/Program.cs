using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LifeSci360.Identity.API.Data;
using LifeSci360.Identity.API.Models;
using LifeSci360.Identity.API.Services;      // ✅ ADD
using LifeSci360.Shared.Services;             // ✅ ADD

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers(); 
builder.Services.AddControllers();
builder.Services.AddScoped<IProtocolService, ProtocolService>(); 

// -------------------- DATABASE --------------------
builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

// -------------------- IDENTITY --------------------
builder.Services.AddIdentity<User, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 10;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppIdentityDbContext>()
.AddDefaultTokenProviders();

// -------------------- COOKIE SETTINGS --------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(3);
    options.SlidingExpiration = true;
});

// -------------------- ✅ REQUIRED SERVICE REGISTRATION ✅ --------------------
builder.Services.AddScoped<IPatientService, PatientService>();

var app = builder.Build();

// -------------------- ROLE + ADMIN SEED --------------------
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    string[] roles =
    {
        "Admin",
        "ResearchScientist",
        "LabTechnician",
        "ClinicalTrialManager",
        "RegulatoryOfficer",
        "DataManager"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = role,
                Description = $"{role} role for LifeSci360",
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            });
        }
    }

    var adminEmail = "admin@lifesci360.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        var admin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Admin",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@12345!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

// -------------------- PIPELINE --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();  
app.UseRouting();      

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); 

app.Run();