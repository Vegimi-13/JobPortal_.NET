using JobPortal_ServerSide.Data;
using JobPortal_ServerSide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;

namespace JobPortal_ServerSide.Controllers
{
    [Authorize(Roles = "Admin,Company")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Company> query = _context.Companies.Include(c => c.User);

            if (!isAdmin)
            {
                query = query.Where(c => c.UserId == userId);
            }

            return View(await query.ToListAsync());
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (company == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (company.UserId != userId)
                    return Forbid();
            }

            return View(company);
        }

        // GET: Companies/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exists = await _context.Companies.AnyAsync(c => c.UserId == userId);
            if (exists)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // POST: Companies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Website,Location")] Company company)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Set UserId BEFORE validation
            company.UserId = userId;
            company.IsActive = true;

            // Remove UserId from ModelState since we set it server-side
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }
                return View(company);
            }

            try
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Company created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during save: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while saving. Please try again.");
                return View(company);
            }
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (company.UserId != userId)
                    return Forbid();
            }

            return View(company);
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Website,Location")] Company company)
        {
            if (id != company.Id) return NotFound();

            var existing = await _context.Companies.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existing.UserId != userId)
                    return Forbid();
            }

            company.UserId = existing.UserId;
            company.IsActive = existing.IsActive;

            try
            {
                _context.Update(company);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Companies.Any(e => e.Id == company.Id))
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

        // GET: Companies/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (company == null) return NotFound();

            return View(company);
        }

        // POST: Companies/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
