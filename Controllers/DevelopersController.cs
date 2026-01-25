using JobPortal_ServerSide.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal_ServerSide.Controllers
{
    public class DevelopersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DevelopersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Developers
        public async Task<IActionResult> Index(string search, string location, string sort)
        {
            var query = _context.DeveloperProfiles
                .Include(d => d.User)
                .Where(d => d.IsActive);

            // 🔍 Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.FullName.Contains(search) ||
                    d.Title.Contains(search));
            }

            // 📍 Filter
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(d => d.Location == location);
            }

            // 🔃 Sort
            query = sort switch
            {
                "name_desc" => query.OrderByDescending(d => d.FullName),
                _ => query.OrderBy(d => d.FullName)
            };

            ViewBag.Locations = await _context.DeveloperProfiles
                .Where(d => d.IsActive && d.Location != null)
                .Select(d => d.Location)
                .Distinct()
                .ToListAsync();

            return View(await query.ToListAsync());
        }

        // GET: /Developers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dev = await _context.DeveloperProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (dev == null)
                return NotFound();

            return View(dev);
        }
    }
}
