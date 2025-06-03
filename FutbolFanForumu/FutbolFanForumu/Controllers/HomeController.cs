using FutbolFanForumu.Data;         // ApplicationDbContext i�in
using FutbolFanForumu.Models;       // LatestThreadViewModel ve ErrorViewModel i�in
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // FromSqlRaw, ToListAsync gibi EF Core metotlar� i�in
using Microsoft.Extensions.Logging; // ILogger i�in
using System.Diagnostics;
using System.Linq;                  // ToListAsync i�in (EF Core 5 veya �ncesi ise, EF Core 6+ i�in genellikle gerekmez)
using System.Threading.Tasks;       // async Task i�in

namespace FutbolFanForumu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // DbContext alan�

        // Constructor ile DbContext ve ILogger enjeksiyonu
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Enjekte edilen DbContext atan�yor
        }

        public async Task<IActionResult> Index()
        {
            // Ana sayfada g�sterilecek son ba�l�k say�s�
            int numberOfThreadsToShow = 5;
            List<LatestThreadViewModel> latestThreads = new List<LatestThreadViewModel>(); // Bo� liste ile ba�lat

            try
            {
                // Stored procedure'� �a��rma
                // FromSqlRaw kullan�rken dikkatli olun, SQL Injection'a a��k olabilir e�er parametreler kullan�c�dan dinamik geliyorsa.
                // Bizim �rne�imizde @Count parametresi sabit bir de�erden (numberOfThreadsToShow) geliyor, bu y�zden g�venli.
                latestThreads = await _context.Set<LatestThreadViewModel>() // EF Core'a SP sonucunu bu tipe map etmesini s�yl�yoruz
                                        .FromSqlRaw("EXEC dbo.GetLatestForumThreads @Count = {0}", numberOfThreadsToShow)
                                        .ToListAsync();
            }
            catch (Exception ex)
            {
                // Stored procedure �a�r�l�rken bir hata olursa logla (veya kullan�c�ya bir mesaj g�ster)
                _logger.LogError(ex, "GetLatestForumThreads stored procedure �a�r�l�rken hata olu�tu.");
                // Hata durumunda bo� bir liste view'a gider, view bunu uygun �ekilde i�lemeli (�rn: "Veri y�klenemedi" mesaj�).
            }

            ViewData["LatestThreads"] = latestThreads; // View'a g�ndermek i�in ViewData kulland�k
            return View();
        }

        public IActionResult Privacy()
        {
            // Gizlilik sayfas� i�in bir view d�nd�r�r (Views/Home/Privacy.cshtml)
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Hata sayfas� i�in ErrorViewModel ile bir view d�nd�r�r (Views/Shared/Error.cshtml)
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}