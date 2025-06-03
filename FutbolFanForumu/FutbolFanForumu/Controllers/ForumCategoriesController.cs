using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FutbolFanForumu.Data;
using FutbolFanForumu.Models;
using Microsoft.AspNetCore.Authorization; // <<<--- BU SATIRI EKLE

namespace FutbolFanForumu.Controllers
{
    [Authorize(Roles = "Admin")] // <<<--- BU SATIRI EKLE (Sınıf seviyesinde yetkilendirme)
    public class ForumCategoriesController : Controller
    {
        // ... (geri kalan kod aynı) ...
        private readonly ApplicationDbContext _context;

        public ForumCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ForumCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.ForumCategories.ToListAsync());
        }

        // ... (Details, Create GET/POST, Edit GET/POST, Delete GET/POST, ForumCategoryExists metodları aynı kalacak) ...
        // Not: Bu metodların içindeki kodları bir önceki mesajlarımdan veya scaffold edilen halinden alabilirsin.
        // Eğer bu controller'ın tam kodunu istersen onu da verebilirim.
        // Şimdilik sadece [Authorize(Roles="Admin")] eklemesini gösterdim.
        // Örnek olarak Create POST metodunu ekleyelim (TempData mesajı ile):
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,IconClass")] ForumCategory forumCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(forumCategory);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Kategori başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            return View(forumCategory);
        }
        // DİĞER TÜM CRUD METODLARI BURADA OLMALI (Index, Details, Edit GET/POST, Delete GET/POST)
        // Eğer bu metodları daha önce sildiysen veya eksikse, scaffold edilen varsayılan hallerini
        // veya daha önceki mesajlarımdaki ikonlu hallerini buraya eklemelisin.
        // Şimdilik sadece [Authorize(Roles="Admin")] attribute'unun nereye ekleneceğini gösteriyorum.
        // Metodların tamamını buraya eklemiyorum çünkü çok uzun olacak.
        // Önemli olan sınıfın başına [Authorize(Roles = "Admin")] eklemen.

        // GET: ForumCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return NotFound();
            }

            return View(forumCategory);
        }

        // GET: ForumCategories/Create (Bu zaten var)
        public IActionResult Create()
        {
            return View();
        }

        // GET: ForumCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories.FindAsync(id);
            if (forumCategory == null)
            {
                return NotFound();
            }
            return View(forumCategory);
        }

        // POST: ForumCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IconClass")] ForumCategory forumCategory)
        {
            if (id != forumCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(forumCategory);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Kategori başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ForumCategoryExists(forumCategory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(forumCategory);
        }

        // GET: ForumCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return NotFound();
            }

            return View(forumCategory);
        }

        // POST: ForumCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var forumCategory = await _context.ForumCategories.FindAsync(id);
            if (forumCategory != null)
            {
                _context.ForumCategories.Remove(forumCategory);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Kategori başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ForumCategoryExists(int id)
        {
            return _context.ForumCategories.Any(e => e.Id == id);
        }
    }
}