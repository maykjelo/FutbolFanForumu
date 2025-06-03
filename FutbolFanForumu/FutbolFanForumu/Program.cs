using FutbolFanForumu.Data;
using Microsoft.AspNetCore.Identity; // RoleManager, UserManager, IdentityRole, IdentityUser için
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) // RequireConfirmedAccount'u false yaptýk
    .AddRoles<IdentityRole>() // <<<--- ROLLER ÝÇÝN BU SATIR EKLENDÝ/GÜNCELLENDÝ
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
// builder.Services.AddRazorPages(); // AddDefaultIdentity bunu zaten ayarlar.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Bu satýrýn olduðundan emin ol
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


// --- ROLLERÝ VE ÖRNEK ADMÝNÝ OLUÞTURMA/ATAMA (SADECE ÝLK ÇALIÞTIRMADA GEREKLÝ OLABÝLÝR) ---
// Bu scope, RoleManager ve UserManager servislerini almak için gerekli.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); // Loglama için

    string[] roleNames = { "Admin", "User" };
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (roleResult.Succeeded)
            {
                logger.LogInformation($"'{roleName}' rolü baþarýyla oluþturuldu.");
            }
            else
            {
                foreach (var error in roleResult.Errors)
                {
                    logger.LogError($"'{roleName}' rolü oluþturulurken hata: {error.Description}");
                }
            }
        }
        else
        {
            logger.LogInformation($"'{roleName}' rolü zaten mevcut.");
        }
    }

    // ÖNEMLÝ: Aþaðýdaki admin kullanýcýsýný atama bloðunu kendi bilgilerine göre düzenle
    // ve bu kod bir kez çalýþýp kullanýcýyý role atadýktan sonra YORUM SATIRINA AL veya SÝL.
    // Her uygulama baþlangýcýnda çalýþmasýna gerek yok.
    /*
    var adminUserEmailToAssign = "SENIN_ADMIN_EMAILIN@example.com"; // <<<--- KENDÝ E-POSTANI YAZ (SÝSTEMDE KAYITLI OLMALI)
    var adminUser = await userManager.FindByEmailAsync(adminUserEmailToAssign);
    if (adminUser != null)
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (addToRoleResult.Succeeded)
            {
                logger.LogInformation($"Kullanýcý '{adminUserEmailToAssign}', 'Admin' rolüne baþarýyla atandý.");
            }
            else
            {
                foreach (var error in addToRoleResult.Errors)
                {
                    logger.LogError($"Kullanýcý '{adminUserEmailToAssign}', 'Admin' rolüne atanýrken hata: {error.Description}");
                }
            }
        }
        else
        {
            logger.LogInformation($"Kullanýcý '{adminUserEmailToAssign}' zaten 'Admin' rolünde.");
        }
    }
    else
    {
        logger.LogWarning($"Admin rolüne atanacak kullanýcý '{adminUserEmailToAssign}' bulunamadý.");
    }
    */
}
// --- ROLLERÝ VE ÖRNEK ADMÝNÝ OLUÞTURMA/ATAMA BÝTÝÞÝ ---

app.Run();