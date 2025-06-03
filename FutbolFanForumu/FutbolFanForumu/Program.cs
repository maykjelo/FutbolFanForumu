using FutbolFanForumu.Data;
using Microsoft.AspNetCore.Identity; // RoleManager, UserManager, IdentityRole, IdentityUser i�in
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false) // RequireConfirmedAccount'u false yapt�k
    .AddRoles<IdentityRole>() // <<<--- ROLLER ���N BU SATIR EKLEND�/G�NCELLEND�
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

app.UseAuthentication(); // Bu sat�r�n oldu�undan emin ol
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


// --- ROLLER� VE �RNEK ADM�N� OLU�TURMA/ATAMA (SADECE �LK �ALI�TIRMADA GEREKL� OLAB�L�R) ---
// Bu scope, RoleManager ve UserManager servislerini almak i�in gerekli.
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); // Loglama i�in

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
                logger.LogInformation($"'{roleName}' rol� ba�ar�yla olu�turuldu.");
            }
            else
            {
                foreach (var error in roleResult.Errors)
                {
                    logger.LogError($"'{roleName}' rol� olu�turulurken hata: {error.Description}");
                }
            }
        }
        else
        {
            logger.LogInformation($"'{roleName}' rol� zaten mevcut.");
        }
    }

    // �NEML�: A�a��daki admin kullan�c�s�n� atama blo�unu kendi bilgilerine g�re d�zenle
    // ve bu kod bir kez �al���p kullan�c�y� role atad�ktan sonra YORUM SATIRINA AL veya S�L.
    // Her uygulama ba�lang�c�nda �al��mas�na gerek yok.
    /*
    var adminUserEmailToAssign = "SENIN_ADMIN_EMAILIN@example.com"; // <<<--- KEND� E-POSTANI YAZ (S�STEMDE KAYITLI OLMALI)
    var adminUser = await userManager.FindByEmailAsync(adminUserEmailToAssign);
    if (adminUser != null)
    {
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var addToRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (addToRoleResult.Succeeded)
            {
                logger.LogInformation($"Kullan�c� '{adminUserEmailToAssign}', 'Admin' rol�ne ba�ar�yla atand�.");
            }
            else
            {
                foreach (var error in addToRoleResult.Errors)
                {
                    logger.LogError($"Kullan�c� '{adminUserEmailToAssign}', 'Admin' rol�ne atan�rken hata: {error.Description}");
                }
            }
        }
        else
        {
            logger.LogInformation($"Kullan�c� '{adminUserEmailToAssign}' zaten 'Admin' rol�nde.");
        }
    }
    else
    {
        logger.LogWarning($"Admin rol�ne atanacak kullan�c� '{adminUserEmailToAssign}' bulunamad�.");
    }
    */
}
// --- ROLLER� VE �RNEK ADM�N� OLU�TURMA/ATAMA B�T��� ---

app.Run();