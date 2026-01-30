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
        public async Task<IActionResult> Index(string search, string location, string skill, string sort)
        {
            var query = _context.DeveloperProfiles
                .Include(d => d.User)
                .Include(d => d.DeveloperSkills)
                    .ThenInclude(ds => ds.Skill)
                .Where(d => d.IsActive);

            // 🔍 Search by name or title
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.FullName.Contains(search) ||
                    d.Title.Contains(search));
            }

            // 📍 Filter by location
            if (!string.IsNullOrWhiteSpace(location))
            {
                query = query.Where(d => d.Location == location);
            }

            // 🛠️ Filter by skill
            if (!string.IsNullOrWhiteSpace(skill))
            {
                query = query.Where(d => d.DeveloperSkills.Any(ds => ds.Skill.Name == skill));
            }

            // 🔃 Sort
            query = sort switch
            {
                "name_desc" => query.OrderByDescending(d => d.FullName),
                _ => query.OrderBy(d => d.FullName)
            };

            // Get distinct locations and skills for filters
            ViewBag.Locations = await _context.DeveloperProfiles
                .Where(d => d.IsActive && d.Location != null)
                .Select(d => d.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            ViewBag.Skills = await _context.Skills
                .OrderBy(s => s.Name)
                .ToListAsync();

            // Preserve filter values
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentLocation = location;
            ViewBag.CurrentSkill = skill;
            ViewBag.CurrentSort = sort;

            return View(await query.ToListAsync());
        }

        // GET: /Developers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var dev = await _context.DeveloperProfiles
                .Include(d => d.User)
                .Include(d => d.DeveloperSkills)
                    .ThenInclude(ds => ds.Skill)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (dev == null)
                return NotFound();

            return View(dev);
        }
    }
}
