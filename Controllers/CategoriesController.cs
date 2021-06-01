using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SpoilerBlockerFull.Data;
using SpoilerBlockerFull.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpoilerBlockerFull.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private static readonly HttpClient client = new HttpClient();

        public CategoriesController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var result = _context.SpoilerCategories.Where(c => c.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)).ToListAsync();
            return View(await result);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.SpoilerCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerCategories.Where(c => ((c.Id == category.Id) && (c.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }

            ViewBag.backDrop = category.BackdropPath;

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CoverColor")] Category category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            if ((ModelState.IsValid) && (userId != null))
            {
                category.UserId = userId;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public IActionResult ImportMovie()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportMovie([Bind("Id,Name,TMDbId,CoverColor,PosterPath,BackdropPath,Overview")] Category category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            string uri = $"https://api.themoviedb.org/3/movie/{category.TMDbId}/credits?api_key=fdbd71ab8b2b768e59653d40c256f41f&language=en-US";
            if ((ModelState.IsValid) && (userId != null))
            {
                category.UserId = userId;
                category.Type = "movie";
                category.IsImported = true;
                _context.Add(category);
                await _context.SaveChangesAsync();
                await ImportKeywordsMovie(category.Id, uri);

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        public IActionResult ImportTV()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportTV([Bind("Id,Name,TMDbId,CoverColor,PosterPath,BackdropPath,Overview")] Category category)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            string uri = $"https://api.themoviedb.org/3/tv/{category.TMDbId}/aggregate_credits?api_key=fdbd71ab8b2b768e59653d40c256f41f&language=en-US";
            if ((ModelState.IsValid) && (userId != null))
            {
                category.UserId = userId;
                category.Type = "tv";
                category.IsImported = true;
                _context.Add(category);
                await _context.SaveChangesAsync();
                await ImportKeywordsTV(category.Id, uri);

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public class MovieCharacter
        {
            [JsonPropertyName("character")]
            public string ch { get; set; }
            [JsonPropertyName("name")]
            public string name { get; set; }
        }
        public class MovieCast
        {
            [JsonPropertyName("cast")]
            public List<MovieCharacter> characters { get; set; }
        }
        public async Task ImportKeywordsMovie(int categotyId, string uri)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var streamTask = client.GetStreamAsync(uri);
            var cast = await JsonSerializer.DeserializeAsync<MovieCast>(await streamTask);

            foreach (var character in cast.characters)
            {
                if (character.ch == "")
                    continue;
                if (character.ch.Contains("#"))
                    continue;
                character.ch = character.ch.Replace(" (uncredited)", "");
                character.ch = character.ch.Replace(" (voice)", "");
                try
                {
                    while (character.ch.IndexOf("/") != -1)
                    {
                        string b = character.ch.Substring(0, character.ch.IndexOf("/"));
                        character.ch = character.ch.Substring(character.ch.IndexOf("/") + 2);
                        Keyword keywordTmp = new Keyword();
                        keywordTmp.CategoryId = categotyId;
                        keywordTmp.Name = b;
                        _context.SpoilerKeywords.Add(keywordTmp);
                    }
                    Keyword keyword = new Keyword();
                    keyword.CategoryId = categotyId;
                    keyword.Name = character.ch;
                    _context.SpoilerKeywords.Add(keyword);
                }
                catch(Exception e)
                {

                }
            }
            await _context.SaveChangesAsync();
        }

        public class TvRole
        {
            [JsonPropertyName("character")]
            public string character { get; set; }
        }
        public class TvCharacter
        {
            [JsonPropertyName("roles")]
            public List<TvRole> roles { get; set; }
            [JsonPropertyName("name")]
            public string name { get; set; }
        }
        public class TvCast
        {
            [JsonPropertyName("cast")]
            public List<TvCharacter> characters { get; set; }
        }
        public async Task ImportKeywordsTV(int categotyId, string uri)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var streamTask = client.GetStreamAsync(uri);
            var cast = await JsonSerializer.DeserializeAsync<TvCast>(await streamTask);
            int counter = 0;
            foreach (var character in cast.characters)
            {
                foreach (var role in character.roles)
                {
                    if (role.character == "")
                        continue;
                    if (role.character.Contains("#"))
                        continue;
                    role.character = role.character.Replace(" (uncredited)", "");
                    role.character = role.character.Replace(" (voice)", "");
                    try
                    {
                        while (role.character.IndexOf("/") != -1)
                        {
                            string b = role.character.Substring(0, role.character.IndexOf("/"));
                            role.character = role.character.Substring(role.character.IndexOf("/") + 2);
                            Keyword keywordTmp = new Keyword();
                            keywordTmp.CategoryId = categotyId;
                            keywordTmp.Name = b;
                            _context.SpoilerKeywords.Add(keywordTmp);
                        }
                        Keyword keyword = new Keyword();
                        keyword.CategoryId = categotyId;
                        keyword.Name = role.character;
                        _context.SpoilerKeywords.Add(keyword);
                    }
                    catch (Exception e)
                    {

                    }
                }
                counter++;
                if (counter > 100)
                    break;
            }
            await _context.SaveChangesAsync();
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.SpoilerCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerCategories.Where(c => ((c.Id == category.Id) && (c.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,UserId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.SpoilerCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var test = _context.SpoilerCategories.Where(c => ((c.Id == category.Id) && (c.UserId != userId))).Count();
            if (test > 0)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.SpoilerCategories.FindAsync(id);
            _context.SpoilerCategories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.SpoilerCategories.Any(e => e.Id == id);
        }
    }
}
