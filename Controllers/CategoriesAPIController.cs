using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpoilerBlockerFull.Data;
using SpoilerBlockerFull.Models;

namespace SpoilerBlockerFull.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesAPIController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSpoilers(string accessToken)
        {
            var res = _context.SpoilerCategories.Where(c => c.User.Id == accessToken).Include(c => c.Keywords).Select(c => new { 
                Name = c.Name, 
                Keywords = _context.SpoilerKeywords.Where(k => k.CategoryId == c.Id).Select(k => k.Name).ToList() 
            });
            return await res.ToListAsync();
        }

    }
}
