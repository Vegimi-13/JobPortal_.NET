using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobPortal_ServerSide.Data;
using JobPortal_ServerSide.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace JobPortal_ServerSide.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class DeveloperProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeveloperProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DeveloperProfiles
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            IQueryable<DeveloperProfile> query = _context.DeveloperProfiles.Include(d => d.User);

            if (!isAdmin)
            {
                query = query.Where(d => d.UserId == userId);
            }

            return View(await query.ToListAsync());
        }


        // GET: DeveloperProfiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DeveloperProfiles == null)
            {
                return NotFound();
            }

            var developerProfile = await _context.DeveloperProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (developerProfile == null)
            {
                return NotFound();
            }

            return View(developerProfile);
        }

        // GET: DeveloperProfiles/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exists = await _context.DeveloperProfiles
                .AnyAsync(d => d.UserId == userId);

            if (exists)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // POST: DeveloperProfiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Title,Bio,Location")] DeveloperProfile developerProfile)
        {
            // Remove UserId from validation since we set it in the controller
            ModelState.Remove("UserId");
            
            Console.WriteLine("🔥 POST CREATE HIT 🔥");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"UserId from claims: {User.FindFirstValue(ClaimTypes.NameIdentifier)}");
            
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"❌ FIELD: {entry.Key}  ERROR: {error.ErrorMessage}");
                }
            }
            
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                developerProfile.UserId = userId;

                Console.WriteLine($"✅ Saving profile: {developerProfile.FullName}");
                _context.Add(developerProfile);
                await _context.SaveChangesAsync();
                Console.WriteLine("✅ SAVED TO DATABASE!");
                return RedirectToAction(nameof(Index));
            }
            
            Console.WriteLine("❌ ModelState INVALID - not saving");
            return View(developerProfile);
        }

        // GET: DeveloperProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var profile = await _context.DeveloperProfiles.FindAsync(id);
            if (profile == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (profile.UserId != userId)
                    return Forbid();
            }

            return View(profile);
        }


        // POST: DeveloperProfiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Title,Bio,Location")] DeveloperProfile developerProfile)
        {
            // Remove UserId from validation since we preserve it from existing record
            ModelState.Remove("UserId");
            
            if (id != developerProfile.Id)
                return NotFound();

            var existing = await _context.DeveloperProfiles.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existing == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existing.UserId != userId)
                    return Forbid();
            }

            developerProfile.UserId = existing.UserId;
            developerProfile.IsActive = existing.IsActive;

            _context.Update(developerProfile);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DeveloperProfiles/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DeveloperProfiles == null)
            {
                return NotFound();
            }

            var developerProfile = await _context.DeveloperProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (developerProfile == null)
            {
                return NotFound();
            }

            return View(developerProfile);
        }

        // POST: DeveloperProfiles/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DeveloperProfiles == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DeveloperProfiles'  is null.");
            }
            var developerProfile = await _context.DeveloperProfiles.FindAsync(id);
            if (developerProfile != null)
            {
                _context.DeveloperProfiles.Remove(developerProfile);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeveloperProfileExists(int id)
        {
          return (_context.DeveloperProfiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
