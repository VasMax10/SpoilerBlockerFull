using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpoilerBlockerFull.Data;
using SpoilerBlockerFull.Models;

namespace SpoilerBlockerFull.Controllers
{
    public class KeywordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public KeywordsController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Keywords
        public async Task<IActionResult> Index(int? categoryId)
        {
            var result = _context.SpoilerKeywords.Where(k => k.CategoryId == categoryId);
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerKeywords.Include(k => k.Category).Where(k => ((k.CategoryId == categoryId) && (k.Category.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }
            ViewBag.categoryId = categoryId;
            ViewBag.categoryName = _context.SpoilerCategories.Where(c => c.Id == categoryId).FirstOrDefault().Name;
            ViewBag.backDrop = _context.SpoilerCategories.Where(c => c.Id == categoryId).FirstOrDefault().BackdropPath;
            return View(await result.ToListAsync());
        }

        // GET: Keywords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var keyword = await _context.SpoilerKeywords
                .Include(k => k.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (keyword == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerKeywords.Include(k => k.Category).Where(k => ((k.CategoryId == keyword.CategoryId) && (k.Category.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }
            ViewBag.CategoryId = keyword.CategoryId;
            return View(keyword);
        }

        // GET: Keywords/Create
        public IActionResult Create(int? categoryId)
        {

            //ViewData["CategoryId"] = categoryId;//new SelectList(_context.SpoilerCategories, "Id", "Id");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerKeywords.Include(k => k.Category).Where(k => ((k.CategoryId == categoryId) && (k.Category.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }
            ViewBag.CategoryId = categoryId;
            return View();
        }

        // POST: Keywords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CategoryId")] Keyword keyword)
        {
            if (ModelState.IsValid)
            {
                _context.Add(keyword);
                await _context.SaveChangesAsync();
                //return RedirectToAction(nameof(Index));
                return RedirectToAction("Index", "Keywords", new { categoryId = keyword.CategoryId });
            }
            ViewData["CategoryId"] = new SelectList(_context.SpoilerCategories, "Id", "Id", keyword.CategoryId);
            return RedirectToAction("Index", "Keywords", new { categoryId = keyword.CategoryId });
            //return View(keyword);
        }

        // GET: Keywords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var keyword = await _context.SpoilerKeywords.FindAsync(id);
            if (keyword == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerKeywords.Include(k => k.Category).Where(k => ((k.CategoryId == keyword.CategoryId) && (k.Category.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }
            //ViewData["CategoryId"] = new SelectList(_context.SpoilerCategories, "Id", "Id", keyword.CategoryId);
            ViewBag.CategoryId = keyword.CategoryId;
            return View(keyword);
        }

        // POST: Keywords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CategoryId")] Keyword keyword)
        {
            if (id != keyword.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(keyword);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KeywordExists(keyword.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Keywords", new { categoryId = keyword.CategoryId });
                //return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.SpoilerCategories, "Id", "Id", keyword.CategoryId);
            return RedirectToAction("Index", "Keywords", new { categoryId = keyword.CategoryId });
            //return View(keyword);
        }

        // GET: Keywords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var keyword = await _context.SpoilerKeywords
                .Include(k => k.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (keyword == null)
            {
                return NotFound();
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerKeywords.Include(k => k.Category).Where(k => ((k.CategoryId == keyword.CategoryId) && (k.Category.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }
            ViewBag.CategoryId = keyword.CategoryId;
            return View(keyword);
        }

        // POST: Keywords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var keyword = await _context.SpoilerKeywords.FindAsync(id);
            _context.SpoilerKeywords.Remove(keyword);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Keywords", new { categoryId = keyword.CategoryId });
            //return RedirectToAction(nameof(Index));
        }

        private bool KeywordExists(int id)
        {
            return _context.SpoilerKeywords.Any(e => e.Id == id);
        }
    }
}
