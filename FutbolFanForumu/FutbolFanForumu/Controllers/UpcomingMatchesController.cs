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
    public class UpcomingMatchesController : Controller
    {
        // ... (geri kalan kod aynı) ...
        // ÖNEMLİ: Bu controller'ın da Index, Details, Create, Edit, Delete metodlarının
        // tam ve çalışır halde olması gerekir. Şimdilik sadece [Authorize] eklemesini gösteriyorum.
        // Metodların içine TempData mesajlarını da ekleyebilirsin.
        // Örnek olarak Create POST metodunu ekleyelim:
        private readonly ApplicationDbContext _context;

        public UpcomingMatchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UpcomingMatches
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UpcomingMatches.Include(u => u.RelatedForumThread);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: UpcomingMatches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var upcomingMatch = await _context.UpcomingMatches
                .Include(u => u.RelatedForumThread)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (upcomingMatch == null)
            {
                return NotFound();
            }

            return View(upcomingMatch);
        }

        // GET: UpcomingMatches/Create
        public IActionResult Create()
        {
            // Eğer RelatedForumThreadId için bir dropdown istiyorsan burada doldurmalısın
            // ViewData["RelatedForumThreadId"] = new SelectList(_context.ForumThreads, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HomeTeamName,AwayTeamName,MatchDateTime,CompetitionName,RelatedForumThreadId")] UpcomingMatch upcomingMatch)
        {
            if (ModelState.IsValid)
            {
                _context.Add(upcomingMatch);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Maç başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            // ViewData["RelatedForumThreadId"] = new SelectList(_context.ForumThreads, "Id", "Title", upcomingMatch.RelatedForumThreadId);
            return View(upcomingMatch);
        }

        // DİĞER CRUD METODLARI (Edit GET/POST, Delete GET/POST) BURADA OLMALI
        // GET: UpcomingMatches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var upcomingMatch = await _context.UpcomingMatches.FindAsync(id);
            if (upcomingMatch == null)
            {
                return NotFound();
            }
            // ViewData["RelatedForumThreadId"] = new SelectList(_context.ForumThreads, "Id", "Title", upcomingMatch.RelatedForumThreadId);
            return View(upcomingMatch);
        }

        // POST: UpcomingMatches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HomeTeamName,AwayTeamName,MatchDateTime,CompetitionName,RelatedForumThreadId")] UpcomingMatch upcomingMatch)
        {
            if (id != upcomingMatch.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(upcomingMatch);
                    await _context.SaveChangesAsync();
                    TempData["Message"] = "Maç başarıyla güncellendi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UpcomingMatchExists(upcomingMatch.Id))
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
            // ViewData["RelatedForumThreadId"] = new SelectList(_context.ForumThreads, "Id", "Title", upcomingMatch.RelatedForumThreadId);
            return View(upcomingMatch);
        }

        // GET: UpcomingMatches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var upcomingMatch = await _context.UpcomingMatches
                .Include(u => u.RelatedForumThread)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (upcomingMatch == null)
            {
                return NotFound();
            }

            return View(upcomingMatch);
        }

        // POST: UpcomingMatches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var upcomingMatch = await _context.UpcomingMatches.FindAsync(id);
            if (upcomingMatch != null)
            {
                _context.UpcomingMatches.Remove(upcomingMatch);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Maç başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UpcomingMatchExists(int id)
        {
            return _context.UpcomingMatches.Any(e => e.Id == id);
        }
    }
}