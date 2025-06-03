using FutbolFanForumu.Data;         // ApplicationDbContext için
using FutbolFanForumu.Models;       // LatestThreadViewModel ve ErrorViewModel için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // FromSqlRaw, ToListAsync gibi EF Core metotlarý için
using Microsoft.Extensions.Logging; // ILogger için
using System.Diagnostics;
using System.Linq;                  // ToListAsync için (EF Core 5 veya öncesi ise, EF Core 6+ için genellikle gerekmez)
using System.Threading.Tasks;       // async Task için

namespace FutbolFanForumu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // DbContext alaný

        // Constructor ile DbContext ve ILogger enjeksiyonu
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // Enjekte edilen DbContext atanýyor
        }

        public async Task<IActionResult> Index()
        {
            // Ana sayfada gösterilecek son baþlýk sayýsý
            int numberOfThreadsToShow = 5;
            List<LatestThreadViewModel> latestThreads = new List<LatestThreadViewModel>(); // Boþ liste ile baþlat

            try
            {
                // Stored procedure'ü çaðýrma
                // FromSqlRaw kullanýrken dikkatli olun, SQL Injection'a açýk olabilir eðer parametreler kullanýcýdan dinamik geliyorsa.
                // Bizim örneðimizde @Count parametresi sabit bir deðerden (numberOfThreadsToShow) geliyor, bu yüzden güvenli.
                latestThreads = await _context.Set<LatestThreadViewModel>() // EF Core'a SP sonucunu bu tipe map etmesini söylüyoruz
                                        .FromSqlRaw("EXEC dbo.GetLatestForumThreads @Count = {0}", numberOfThreadsToShow)
                                        .ToListAsync();
            }
            catch (Exception ex)
            {
                // Stored procedure çaðrýlýrken bir hata olursa logla (veya kullanýcýya bir mesaj göster)
                _logger.LogError(ex, "GetLatestForumThreads stored procedure çaðrýlýrken hata oluþtu.");
                // Hata durumunda boþ bir liste view'a gider, view bunu uygun þekilde iþlemeli (örn: "Veri yüklenemedi" mesajý).
            }

            ViewData["LatestThreads"] = latestThreads; // View'a göndermek için ViewData kullandýk
            return View();
        }

        public IActionResult Privacy()
        {
            // Gizlilik sayfasý için bir view döndürür (Views/Home/Privacy.cshtml)
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Hata sayfasý için ErrorViewModel ile bir view döndürür (Views/Shared/Error.cshtml)
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}