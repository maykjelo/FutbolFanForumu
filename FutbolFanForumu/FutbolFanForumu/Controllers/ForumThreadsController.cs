using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FutbolFanForumu.Data;
using FutbolFanForumu.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
// using Microsoft.AspNetCore.Identity;

namespace FutbolFanForumu.Controllers
{
    public class ForumThreadsController : Controller
    {
        private readonly ApplicationDbContext _context;
        // private readonly UserManager<IdentityUser> _userManager;

        public ForumThreadsController(ApplicationDbContext context /*, UserManager<IdentityUser> userManager */)
        {
            _context = context;
            // _userManager = userManager;
        }

        // GET: ForumThreads
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ForumThreads
                                                .Include(f => f.ForumCategory)
                                                .Include(f => f.User)
                                                .OrderByDescending(f => f.LastPostDate); // Son aktiviteye göre sırala
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ForumThreads/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ForumThreads == null)
            {
                return NotFound();
            }

            var forumThread = await _context.ForumThreads
                .Include(f => f.ForumCategory)
                .Include(f => f.User)
                .Include(f => f.Posts)
                    .ThenInclude(p => p.User) // Yorumların kullanıcılarını da yükle
                .FirstOrDefaultAsync(m => m.Id == id);

            if (forumThread == null)
            {
                return NotFound();
            }

            // Görüntülenme sayısını artır (basit bir implementasyon)
            forumThread.ViewCount++;
            _context.Update(forumThread);
            await _context.SaveChangesAsync(); // Değişikliği hemen kaydet

            return View(forumThread);
        }

        // GET: ForumThreads/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name");
            return View();
        }

        // POST: ForumThreads/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Content,ForumCategoryId")] ForumThread forumThread)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            forumThread.UserId = userId;
            forumThread.CreatedDate = DateTime.Now;
            forumThread.LastPostDate = DateTime.Now; // İlk oluşturulduğunda son aktivite tarihi aynı
            forumThread.ViewCount = 0;

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("LastPostDate");
            ModelState.Remove("ViewCount");
            ModelState.Remove("ForumCategory");
            ModelState.Remove("Posts"); // Posts koleksiyonu için de validasyonu kaldır

            if (ModelState.IsValid)
            {
                _context.Add(forumThread);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Başlığınız başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Details), new { id = forumThread.Id }); // Oluşturulan başlığın detayına git
            }
            else
            {
                var errorMessageBuilder = new StringBuilder();
                errorMessageBuilder.AppendLine("Formda validasyon hataları tespit edildi (Create POST):");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        errorMessageBuilder.AppendFormat("-> Alan: \"{0}\", Hata(lar): ", key);
                        foreach (var error in state.Errors)
                        {
                            errorMessageBuilder.AppendFormat("\"{0}\" ", error.ErrorMessage);
                        }
                        errorMessageBuilder.AppendLine();
                    }
                }
                TempData["ErrorMessageFromValidation"] = errorMessageBuilder.ToString();
            }

            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forumThread.ForumCategoryId);
            return View(forumThread);
        }

        // POST: ForumThreads/AddPost (YENİ EKLENEN METOT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddPost(int ThreadId, string PostContent)
        {
            if (string.IsNullOrWhiteSpace(PostContent))
            {
                TempData["ErrorMessage"] = "Yorum içeriği boş olamaz.";
                return RedirectToAction(nameof(Details), new { id = ThreadId });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var threadExists = await _context.ForumThreads.AnyAsync(t => t.Id == ThreadId);
            if (!threadExists)
            {
                TempData["ErrorMessage"] = "Yorum yapılmak istenen başlık bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ForumPost newPost = new ForumPost
            {
                Content = PostContent,
                PostedDate = DateTime.Now,
                UserId = userId,
                ForumThreadId = ThreadId
            };

            _context.ForumPosts.Add(newPost);

            var threadToUpdate = await _context.ForumThreads.FindAsync(ThreadId);
            if (threadToUpdate != null)
            {
                threadToUpdate.LastPostDate = DateTime.Now;
                _context.Update(threadToUpdate);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Yorumunuz başarıyla eklendi.";
            return RedirectToAction(nameof(Details), new { id = ThreadId });
        }


        // GET: ForumThreads/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ForumThreads == null)
            {
                return NotFound();
            }

            var forumThread = await _context.ForumThreads.FindAsync(id);
            if (forumThread == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (forumThread.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forumThread.ForumCategoryId);
            return View(forumThread);
        }

        // POST: ForumThreads/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ForumCategoryId")] ForumThread forumThread)
        {
            if (id != forumThread.Id)
            {
                return NotFound();
            }

            var originalThread = await _context.ForumThreads.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (originalThread == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (originalThread.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            forumThread.UserId = originalThread.UserId;
            forumThread.CreatedDate = originalThread.CreatedDate;
            forumThread.ViewCount = originalThread.ViewCount;
            forumThread.LastPostDate = DateTime.Now;

            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("LastPostDate");
            ModelState.Remove("ViewCount");
            ModelState.Remove("ForumCategory");
            ModelState.Remove("Posts");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(forumThread);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Başlığınız başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ForumThreadExists(forumThread.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = forumThread.Id }); // Düzenleme sonrası detay sayfasına git
            }
            else
            {
                var errorMessageBuilder = new StringBuilder();
                errorMessageBuilder.AppendLine("Formda validasyon hataları tespit edildi (Edit POST):");
                // ... (hata loglama kısmı aynı kalabilir) ...
                TempData["ErrorMessageFromValidation"] = errorMessageBuilder.ToString();
            }
            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forumThread.ForumCategoryId);
            return View(forumThread);
        }

        // GET: ForumThreads/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ForumThreads == null)
            {
                return NotFound();
            }

            var forumThread = await _context.ForumThreads
                .Include(f => f.ForumCategory)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (forumThread == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (forumThread.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(forumThread);
        }

        // POST: ForumThreads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ForumThreads == null)
            {
                return Problem("Entity set 'ApplicationDbContext.ForumThreads'  is null.");
            }
            var forumThread = await _context.ForumThreads.FindAsync(id);

            if (forumThread != null)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (forumThread.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }
                _context.ForumThreads.Remove(forumThread);
                TempData["Message"] = "Başlık başarıyla silindi.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ForumThreadExists(int id)
        {
            return (_context.ForumThreads?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}